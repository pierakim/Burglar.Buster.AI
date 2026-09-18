using BurglarBuster.Core.Matching;

namespace BurglarBuster.Core.Tools;

/// <summary>
/// Bounded to <see cref="MatchingPolicy.MaxDetailedCandidates" /> IDs — the same limit
/// that governs how many candidates the agent may investigate in detail overall
/// (spec §11).
/// </summary>
public sealed record CompareCandidatesArguments
{
    public required SearchCriteria Criteria { get; init; }

    public required IReadOnlyList<string> PersonIds { get; init; }
}

public sealed record CompareCandidatesResult
{
    /// <summary>Ranked among only the requested IDs — not the wider candidate pool.</summary>
    public required IReadOnlyList<ScoredCandidate> Candidates { get; init; }

    /// <summary>Requested IDs that don't correspond to any seeded record.</summary>
    public required IReadOnlyList<string> UnknownPersonIds { get; init; }
}
