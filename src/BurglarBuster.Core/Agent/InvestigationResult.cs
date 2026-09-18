using BurglarBuster.Core.Matching;

namespace BurglarBuster.Core.Agent;

/// <summary>
/// The complete result of one investigation — matches the public HTTP contract shape
/// from spec §13. Outcome/Candidates always come from host-tracked deterministic
/// evidence (the last successful search_people/compare_candidates result), never from
/// the model's own text.
/// </summary>
public sealed record InvestigationResult
{
    public required string InvestigationId { get; init; }

    public required MatchOutcome Outcome { get; init; }

    public required string Summary { get; init; }

    public required IReadOnlyList<ScoredCandidate> Candidates { get; init; }

    public required IReadOnlyList<string> RequestedInformation { get; init; }

    public required bool RequiresHumanReview { get; init; }

    public required TerminationReason TerminationReason { get; init; }

    public required string ModelDeploymentId { get; init; }

    public required int ModelTurnCount { get; init; }

    public required int ToolCallCount { get; init; }
}
