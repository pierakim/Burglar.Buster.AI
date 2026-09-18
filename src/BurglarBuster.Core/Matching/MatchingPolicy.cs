namespace BurglarBuster.Core.Matching;

/// <summary>
/// Centralized weights, thresholds and tolerances for the matching engine — the single
/// place that decides how much any one piece of evidence counts. See
/// BURGLAR_BUSTER_PROJECT_SPEC.md §9 for the rules these values must satisfy:
/// stable identifiers (DOB, name, address history) outweigh appearance; appearance
/// alone can never produce a strong candidate; a strong result needs agreement across
/// more than one independent attribute group; case history never affects score
/// (case references are never read here at all).
/// </summary>
public static class MatchingPolicy
{
    // --- Name group (max 35: exact family 20 + exact given 15) ---
    public const int FamilyNameExactPoints = 20;
    public const int FamilyNameSimilarPoints = 12;
    public const int GivenNameExactPoints = 15;
    public const int GivenNameSimilarPoints = 8;
    public const int AliasExactPoints = 25;
    public const int AliasSimilarPoints = 15;
    public const int NameGroupMaxPoints = 35;

    /// <summary>Max Levenshtein distance for two names to count as "similar" (misspelling).</summary>
    public const int NameSimilarityMaxEditDistance = 2;

    // --- Date of birth / age group (max 30) ---
    public const int ExactDateOfBirthPoints = 30;
    public const int ApproximateAgeWithinTolerancePoints = 12;
    public const int ApproximateAgeToleranceYears = 3;

    // --- Address group (max 20, best single address wins — not summed across addresses) ---
    public const int CurrentAddressLocalityPoints = 12;
    public const int PreviousAddressLocalityPoints = 9;
    public const int PostcodeExactPoints = 6;
    public const int AddressFragmentPoints = 7;
    public const int AddressGroupMaxPoints = 20;

    // --- Appearance group (max 12 — deliberately capped low: supporting evidence only) ---
    public const int ApproximateHeightWithinTolerancePoints = 4;
    public const int ApproximateHeightToleranceCm = 5;
    public const int EyeColourExactPoints = 3;
    public const int HairColourExactPoints = 3;
    public const int DistinguishingMarkMatchPoints = 4;
    public const int AppearanceGroupMaxPoints = 12;

    // --- Candidate pool / result bounds ---
    public const int MaxCandidatePoolSize = 200;
    public const int MaxCandidatesReturned = 5;
    public const int MaxDetailedCandidates = 3;

    // --- Outcome thresholds ---

    /// <summary>Below this score, a person isn't even listed as a candidate.</summary>
    public const int PlausibleCandidateMinScore = 20;

    /// <summary>Minimum score for a candidate to be eligible for strong_candidate.</summary>
    public const int StrongCandidateMinScore = 55;

    /// <summary>
    /// The top candidate must beat the runner-up by at least this many points to be
    /// "materially stronger" rather than merely ahead.
    /// </summary>
    public const int StrongCandidateMinMarginOverSecond = 20;

    /// <summary>
    /// A strong result requires the name group AND the DOB/age group AND the address
    /// group to each contribute independently — appearance never counts toward this.
    /// See "MinIndependentGroupsForStrongMatch" usage in CandidateScorer/MatchingEngine.
    /// </summary>
    public const int MinIndependentGroupsForStrongMatch = 2;

    // --- Duplicate detection (two DB records, not criteria vs. record) ---
    public const int DuplicateNameMaxEditDistance = 2;
}
