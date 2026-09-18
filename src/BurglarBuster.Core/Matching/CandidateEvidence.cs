namespace BurglarBuster.Core.Matching;

/// <summary>
/// Field-level evidence for one candidate: no unsupported prose conclusions, just what
/// matched, what conflicted, and what was unavailable (spec §9).
/// </summary>
public sealed record CandidateEvidence
{
    public required IReadOnlyList<string> MatchedEvidence { get; init; }

    public required IReadOnlyList<string> Conflicts { get; init; }

    public required IReadOnlyList<string> MissingEvidence { get; init; }

    /// <summary>
    /// How many independent groups (name, DOB/age, address — never appearance alone)
    /// contributed evidence. Used by the outcome policy, not shown to the model as a score.
    /// </summary>
    public required int IndependentGroupsMatched { get; init; }

    /// <summary>
    /// True when this person's own records disagree with themselves (e.g. two
    /// physical descriptions with a large, unexplained discrepancy) — independent of
    /// how well they match the search criteria.
    /// </summary>
    public required bool HasInternalInconsistency { get; init; }
}
