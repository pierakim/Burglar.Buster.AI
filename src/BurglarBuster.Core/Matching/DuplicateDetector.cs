using BurglarBuster.Core.Entities;

namespace BurglarBuster.Core.Matching;

/// <summary>
/// Detects when two returned candidates look like they might be the same synthetic
/// person recorded twice — a data-quality question about the database, distinct from
/// how well either one matches the search criteria. Deliberately strict (all of DOB,
/// given name, family name and an address must agree) so it only fires for genuine
/// duplicate-style records, never for two unrelated people who happen to share one
/// attribute (spec §12's potential_duplicate outcome).
/// </summary>
internal static class DuplicateDetector
{
    public static bool AreLikelyDuplicates(Person a, Person b)
    {
        // Not "is this person's own data self-contradictory" (that's
        // InternalConsistencyChecker) — this asks "are these two DIFFERENT database
        // rows suspicious enough to be the same real person entered twice". If the
        // IDs are equal there's only one row being compared to itself, so there's no
        // duplicate *pair* to report; this is a defensive guard against that
        // degenerate case, not a meaningful match.
        if (a.PersonId == b.PersonId)
        {
            return false;
        }

        if (a.DateOfBirth is null || b.DateOfBirth is null || a.DateOfBirth != b.DateOfBirth)
        {
            return false;
        }

        if (!NamesAreSimilar(a.FamilyName, b.FamilyName) || !NamesAreSimilar(a.GivenName, b.GivenName))
        {
            return false;
        }

        return a.Addresses.Any(x => b.Addresses.Any(y =>
            NameNormalizer.Normalize(x.AddressLine) == NameNormalizer.Normalize(y.AddressLine)));
    }

    private static bool NamesAreSimilar(string x, string y)
    {
        var normalizedX = NameNormalizer.Normalize(x);
        var normalizedY = NameNormalizer.Normalize(y);

        return normalizedX == normalizedY
            || LevenshteinDistance.Compute(normalizedX, normalizedY) <= MatchingPolicy.DuplicateNameMaxEditDistance;
    }
}
