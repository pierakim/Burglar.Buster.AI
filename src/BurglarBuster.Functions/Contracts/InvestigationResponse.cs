using BurglarBuster.Core.Agent;
using BurglarBuster.Core.Matching;

namespace BurglarBuster.Functions.Contracts;

/// <summary>
/// Shapes the response to match BURGLAR_BUSTER_PROJECT_SPEC.md §13's documented
/// contract exactly (flat evidence fields per candidate) rather than exposing the
/// internal <see cref="ScoredCandidate" />/<see cref="InvestigationResult" /> shape
/// verbatim. Model/turn/tool-call counts are omitted here — they're already
/// persisted and belong on the fuller GET /api/investigations/{id} view (Milestone 6).
/// </summary>
public sealed record InvestigationResponse
{
    public required string InvestigationId { get; init; }

    public required MatchOutcome Outcome { get; init; }

    public required string Summary { get; init; }

    public required IReadOnlyList<InvestigationCandidateResponse> Candidates { get; init; }

    public required IReadOnlyList<string> RequestedInformation { get; init; }

    public required bool RequiresHumanReview { get; init; }

    public required TerminationReason TerminationReason { get; init; }

    public static InvestigationResponse FromResult(InvestigationResult result) => new()
    {
        InvestigationId = result.InvestigationId,
        Outcome = result.Outcome,
        Summary = result.Summary,
        RequestedInformation = result.RequestedInformation,
        RequiresHumanReview = result.RequiresHumanReview,
        TerminationReason = result.TerminationReason,
        Candidates = result.Candidates.Select(c => new InvestigationCandidateResponse
        {
            PersonId = c.PersonId,
            Rank = c.Rank,
            Score = c.Score,
            MatchedEvidence = c.Evidence.MatchedEvidence,
            Conflicts = c.Evidence.Conflicts,
            MissingEvidence = c.Evidence.MissingEvidence,
        }).ToList(),
    };
}

public sealed record InvestigationCandidateResponse
{
    public required string PersonId { get; init; }

    public required int Rank { get; init; }

    public required int Score { get; init; }

    public required IReadOnlyList<string> MatchedEvidence { get; init; }

    public required IReadOnlyList<string> Conflicts { get; init; }

    public required IReadOnlyList<string> MissingEvidence { get; init; }
}
