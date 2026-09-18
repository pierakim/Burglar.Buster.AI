namespace BurglarBuster.Core.Matching;

/// <summary>
/// The full deterministic result of a search: the outcome and, where applicable, up
/// to <see cref="MatchingPolicy.MaxCandidatesReturned" /> ranked candidates.
/// </summary>
public sealed record MatchResult
{
    public required MatchOutcome Outcome { get; init; }

    public required IReadOnlyList<ScoredCandidate> Candidates { get; init; }

    public static MatchResult Empty(MatchOutcome outcome) => new()
    {
        Outcome = outcome,
        Candidates = [],
    };
}
