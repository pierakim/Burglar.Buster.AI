using BurglarBuster.Core.Entities;

namespace BurglarBuster.Core.Matching;

/// <summary>
/// Pure, deterministic field-by-field comparison between one person and the search
/// criteria. No I/O, no randomness — same inputs always produce the same score and
/// evidence (spec §9's reproducibility requirement). Case references are never read
/// here: case history must not affect identity similarity (spec §9).
/// </summary>
internal static class CandidateScorer
{
    public static (int Score, CandidateEvidence Evidence) Evaluate(Person person, SearchCriteria criteria, DateOnly referenceDate)
    {
        var matched = new List<string>();
        var conflicts = new List<string>();
        var missing = new List<string>();

        var (nameScore, nameGroupMatched) = EvaluateName(person, criteria, matched, conflicts);
        var (dobScore, dobGroupMatched) = EvaluateDateOfBirth(person, criteria, referenceDate, matched, conflicts, missing);
        var (addressScore, addressGroupMatched) = EvaluateAddress(person, criteria, matched, conflicts, missing);
        var appearanceScore = EvaluateAppearance(person, criteria, matched, conflicts, missing);

        var score = nameScore + dobScore + addressScore + appearanceScore;

        var independentGroups = (nameGroupMatched ? 1 : 0) + (dobGroupMatched ? 1 : 0) + (addressGroupMatched ? 1 : 0);

        var evidence = new CandidateEvidence
        {
            MatchedEvidence = matched,
            Conflicts = conflicts,
            MissingEvidence = missing,
            IndependentGroupsMatched = independentGroups,
            HasInternalInconsistency = InternalConsistencyChecker.GetInternalConflicts(person).Count > 0,
        };

        return (score, evidence);
    }

    private static (int Points, bool Matched) EvaluateName(Person person, SearchCriteria criteria, List<string> matched, List<string> conflicts)
    {
        var candidates = new List<(int Points, string Evidence)>();

        // Primary name: given + family compared independently, so a partial match still counts.
        var givenPoints = 0;
        var familyPoints = 0;

        if (!string.IsNullOrWhiteSpace(criteria.GivenName))
        {
            givenPoints = CompareNames(criteria.GivenName, person.GivenName, MatchingPolicy.GivenNameExactPoints, MatchingPolicy.GivenNameSimilarPoints);
            if (givenPoints == 0)
            {
                conflicts.Add($"given name differs (stated \"{criteria.GivenName}\", recorded \"{person.GivenName}\")");
            }
        }

        if (!string.IsNullOrWhiteSpace(criteria.FamilyName))
        {
            familyPoints = CompareNames(criteria.FamilyName, person.FamilyName, MatchingPolicy.FamilyNameExactPoints, MatchingPolicy.FamilyNameSimilarPoints);
            if (familyPoints == 0)
            {
                conflicts.Add($"family name differs (stated \"{criteria.FamilyName}\", recorded \"{person.FamilyName}\")");
            }
        }

        if (givenPoints > 0 || familyPoints > 0)
        {
            var parts = new List<string>();
            if (givenPoints > 0) parts.Add("given name");
            if (familyPoints > 0) parts.Add("family name");
            candidates.Add((Math.Min(givenPoints + familyPoints, MatchingPolicy.NameGroupMaxPoints), string.Join(" and ", parts)));
        }

        // Combined full name against the primary name.
        if (!string.IsNullOrWhiteSpace(criteria.FullName))
        {
            var personFullName = $"{person.GivenName} {person.FamilyName}";
            var points = CompareNames(criteria.FullName, personFullName, MatchingPolicy.NameGroupMaxPoints, MatchingPolicy.FamilyNameSimilarPoints + MatchingPolicy.GivenNameSimilarPoints);
            if (points > 0)
            {
                candidates.Add((points, "full name"));
            }
        }

        // Aliases: check the alias-specific field, and also fall back to given/family/full
        // criteria in case the operator described the person by an alias without saying so.
        foreach (var alias in person.Aliases)
        {
            var bestAliasPoints = 0;

            if (!string.IsNullOrWhiteSpace(criteria.AliasName))
            {
                bestAliasPoints = Math.Max(bestAliasPoints, CompareNames(criteria.AliasName, alias.FullName, MatchingPolicy.AliasExactPoints, MatchingPolicy.AliasSimilarPoints));
            }

            if (!string.IsNullOrWhiteSpace(criteria.FullName))
            {
                bestAliasPoints = Math.Max(bestAliasPoints, CompareNames(criteria.FullName, alias.FullName, MatchingPolicy.AliasExactPoints, MatchingPolicy.AliasSimilarPoints));
            }

            if (bestAliasPoints > 0)
            {
                candidates.Add((bestAliasPoints, "alias"));
            }
        }

        if (candidates.Count == 0)
        {
            return (0, false);
        }

        var best = candidates.MaxBy(c => c.Points);
        matched.Add(best.Evidence);
        return (best.Points, true);
    }

    private static int CompareNames(string criteriaValue, string recordValue, int exactPoints, int similarPoints)
    {
        var a = NameNormalizer.Normalize(criteriaValue);
        var b = NameNormalizer.Normalize(recordValue);

        if (a.Length == 0 || b.Length == 0)
        {
            return 0;
        }

        if (a == b)
        {
            return exactPoints;
        }

        return LevenshteinDistance.Compute(a, b) <= MatchingPolicy.NameSimilarityMaxEditDistance
            ? similarPoints
            : 0;
    }

    private static (int Points, bool Matched) EvaluateDateOfBirth(Person person, SearchCriteria criteria, DateOnly referenceDate, List<string> matched, List<string> conflicts, List<string> missing)
    {
        if (criteria.DateOfBirth is { } statedDob)
        {
            if (person.DateOfBirth is not { } recordedDob)
            {
                missing.Add("date of birth");
                return (0, false);
            }

            if (recordedDob == statedDob)
            {
                matched.Add("date of birth");
                return (MatchingPolicy.ExactDateOfBirthPoints, true);
            }

            conflicts.Add($"date of birth differs (stated {statedDob:yyyy-MM-dd}, recorded {recordedDob:yyyy-MM-dd})");
            return (0, false);
        }

        if (criteria.ApproximateAge is { } statedAge)
        {
            if (person.DateOfBirth is not { } recordedDob2)
            {
                missing.Add("date of birth");
                return (0, false);
            }

            var recordedAge = CalculateAge(recordedDob2, referenceDate);
            if (Math.Abs(recordedAge - statedAge) <= MatchingPolicy.ApproximateAgeToleranceYears)
            {
                matched.Add("approximate age");
                return (MatchingPolicy.ApproximateAgeWithinTolerancePoints, true);
            }

            conflicts.Add($"age differs from the stated approximate age (recorded age ~{recordedAge}, stated ~{statedAge})");
            return (0, false);
        }

        return (0, false);
    }

    private static (int Points, bool Matched) EvaluateAddress(Person person, SearchCriteria criteria, List<string> matched, List<string> conflicts, List<string> missing)
    {
        var anyCriteria = criteria.AddressFragment is not null || criteria.Locality is not null
            || criteria.State is not null || criteria.Postcode is not null;

        if (!anyCriteria)
        {
            return (0, false);
        }

        if (person.Addresses.Count == 0)
        {
            missing.Add("address");
            return (0, false);
        }

        var bestPoints = 0;
        PersonAddress? bestAddress = null;

        foreach (var address in person.Addresses)
        {
            var points = 0;

            if (!string.IsNullOrWhiteSpace(criteria.Locality)
                && NameNormalizer.Normalize(criteria.Locality) == NameNormalizer.Normalize(address.Locality))
            {
                points += address.ValidTo is null ? MatchingPolicy.CurrentAddressLocalityPoints : MatchingPolicy.PreviousAddressLocalityPoints;
            }

            if (!string.IsNullOrWhiteSpace(criteria.Postcode) && criteria.Postcode.Trim() == address.Postcode.Trim())
            {
                points += MatchingPolicy.PostcodeExactPoints;
            }

            if (!string.IsNullOrWhiteSpace(criteria.AddressFragment)
                && NameNormalizer.Normalize(address.AddressLine).Contains(NameNormalizer.Normalize(criteria.AddressFragment), StringComparison.Ordinal))
            {
                points += MatchingPolicy.AddressFragmentPoints;
            }

            points = Math.Min(points, MatchingPolicy.AddressGroupMaxPoints);

            if (points > bestPoints)
            {
                bestPoints = points;
                bestAddress = address;
            }
        }

        if (bestPoints > 0 && bestAddress is not null)
        {
            matched.Add(bestAddress.ValidTo is null ? "current address" : "previous address");
            return (bestPoints, true);
        }

        conflicts.Add("address does not match any recorded address");
        return (0, false);
    }

    private static int EvaluateAppearance(Person person, SearchCriteria criteria, List<string> matched, List<string> conflicts, List<string> missing)
    {
        var points = 0;

        if (criteria.ApproximateHeightCm is { } statedHeight)
        {
            var observed = person.PhysicalDescriptions.Where(d => d.ApproximateHeightCm.HasValue).ToList();
            if (observed.Count == 0)
            {
                missing.Add("height");
            }
            else
            {
                var closest = observed.MinBy(d => Math.Abs(d.ApproximateHeightCm!.Value - statedHeight))!;
                var diff = Math.Abs(closest.ApproximateHeightCm!.Value - statedHeight);
                if (diff <= MatchingPolicy.ApproximateHeightToleranceCm)
                {
                    matched.Add("approximate height");
                    points += MatchingPolicy.ApproximateHeightWithinTolerancePoints;
                }
                else
                {
                    conflicts.Add($"recorded height differs by {diff} cm");
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(criteria.EyeColour))
        {
            var observed = person.PhysicalDescriptions.Where(d => !string.IsNullOrWhiteSpace(d.EyeColour)).ToList();
            if (observed.Count == 0)
            {
                missing.Add("eye colour");
            }
            else if (observed.Any(d => NameNormalizer.Normalize(d.EyeColour) == NameNormalizer.Normalize(criteria.EyeColour)))
            {
                matched.Add("eye colour");
                points += MatchingPolicy.EyeColourExactPoints;
            }
            else
            {
                conflicts.Add($"eye colour differs (stated \"{criteria.EyeColour}\", recorded \"{observed[0].EyeColour}\")");
            }
        }

        if (!string.IsNullOrWhiteSpace(criteria.HairColour))
        {
            var observed = person.PhysicalDescriptions.Where(d => !string.IsNullOrWhiteSpace(d.HairColour)).ToList();
            if (observed.Count == 0)
            {
                missing.Add("hair colour");
            }
            else if (observed.Any(d => NameNormalizer.Normalize(d.HairColour) == NameNormalizer.Normalize(criteria.HairColour)))
            {
                matched.Add("hair colour");
                points += MatchingPolicy.HairColourExactPoints;
            }
            else
            {
                conflicts.Add($"hair colour differs (stated \"{criteria.HairColour}\", recorded \"{observed[0].HairColour}\")");
            }
        }

        if (!string.IsNullOrWhiteSpace(criteria.DistinguishingMark))
        {
            var observed = person.PhysicalDescriptions.Where(d => !string.IsNullOrWhiteSpace(d.DistinguishingMarks)).ToList();
            if (observed.Count == 0)
            {
                missing.Add("distinguishing marks");
            }
            else if (observed.Any(d => NameNormalizer.Normalize(d.DistinguishingMarks).Contains(NameNormalizer.Normalize(criteria.DistinguishingMark), StringComparison.Ordinal)))
            {
                matched.Add("distinguishing mark");
                points += MatchingPolicy.DistinguishingMarkMatchPoints;
            }
        }

        return Math.Min(points, MatchingPolicy.AppearanceGroupMaxPoints);
    }

    private static int CalculateAge(DateOnly dateOfBirth, DateOnly asOf)
    {
        var age = asOf.Year - dateOfBirth.Year;
        if (dateOfBirth > asOf.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
