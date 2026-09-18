namespace BurglarBuster.Core.Matching;

public sealed record ScoredCandidate
{
    public required string PersonId { get; init; }

    /// <summary>Deterministic application-computed score — never a model confidence value.</summary>
    public required int Score { get; init; }

    public required int Rank { get; init; }

    public required CandidateEvidence Evidence { get; init; }
}
