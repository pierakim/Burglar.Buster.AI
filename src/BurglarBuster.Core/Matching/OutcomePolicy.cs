namespace BurglarBuster.Core.Matching;

/// <summary>
/// Whether a search should even be attempted. A description with nothing but
/// appearance details isn't reliable enough to search on — spec §12's
/// insufficient_information outcome.
/// </summary>
public static class OutcomePolicy
{
    public static bool IsSufficient(SearchCriteria criteria)
    {
        var hasNameHint = !string.IsNullOrWhiteSpace(criteria.GivenName)
            || !string.IsNullOrWhiteSpace(criteria.FamilyName)
            || !string.IsNullOrWhiteSpace(criteria.FullName)
            || !string.IsNullOrWhiteSpace(criteria.AliasName);

        var hasDobOrAgeHint = criteria.DateOfBirth.HasValue || criteria.ApproximateAge.HasValue;

        var hasAddressHint = !string.IsNullOrWhiteSpace(criteria.AddressFragment)
            || !string.IsNullOrWhiteSpace(criteria.Locality)
            || !string.IsNullOrWhiteSpace(criteria.State)
            || !string.IsNullOrWhiteSpace(criteria.Postcode);

        return hasNameHint || hasDobOrAgeHint || hasAddressHint;
    }

    /// <summary>
    /// Every candidate-bearing outcome requires a human to actually look at it (spec
    /// §12) — human review is a workflow state, not proof the AI was correct.
    /// no_match/insufficient_information have no candidate to confirm or reject, so
    /// there's nothing for a human to review yet.
    /// </summary>
    public static bool RequiresHumanReview(MatchOutcome outcome) => outcome switch
    {
        MatchOutcome.NoMatch => false,
        MatchOutcome.InsufficientInformation => false,
        _ => true,
    };
}
