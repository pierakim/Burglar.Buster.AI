using BurglarBuster.Core.Agent;
using BurglarBuster.Core.Matching;

namespace BurglarBuster.Core.Entities;

/// <summary>
/// A persisted record of one investigation run. Write-once from the agent loop
/// (Milestone 5) — human review (Milestone 7) records a separate ReviewDecision
/// against this but never mutates it.
/// </summary>
public sealed class Investigation
{
    /// <summary>Stable identifier, e.g. "INV-20260918-ab12cd34".</summary>
    public required string InvestigationId { get; init; }

    /// <summary>The operator's raw description — sanitized/demo-only representation
    /// (spec §7); this is synthetic data end to end so full text is acceptable here.</summary>
    public required string Description { get; init; }

    public required DateTimeOffset CreatedAtUtc { get; init; }

    public required MatchOutcome Outcome { get; init; }

    public required string Summary { get; init; }

    public required bool RequiresHumanReview { get; init; }

    public required string ModelDeploymentId { get; init; }

    public required int ModelTurnCount { get; init; }

    public required int ToolCallCount { get; init; }

    public required TerminationReason TerminationReason { get; init; }

    /// <summary>JSON-encoded string array — empty array when nothing was requested.</summary>
    public required string RequestedInformationJson { get; init; }

    public ICollection<InvestigationCandidate> Candidates { get; init; } = new List<InvestigationCandidate>();
}
