using BurglarBuster.Core.Matching;

namespace BurglarBuster.Infrastructure.Matching;

/// <summary>
/// Wires stage 1 (bounded retrieval) to stage 2 (pure scoring + outcome policy). This
/// is the seam the Milestone 4 tool layer calls — nothing here is agent/tool-specific.
/// </summary>
public sealed class MatchingService(IPersonCandidateRepository repository)
{
    public async Task<MatchResult> SearchAsync(SearchCriteria criteria, DateOnly referenceDate, CancellationToken cancellationToken)
    {
        if (!OutcomePolicy.IsSufficient(criteria))
        {
            return MatchResult.Empty(MatchOutcome.InsufficientInformation);
        }

        var pool = await repository.GetCandidatePoolAsync(criteria, cancellationToken);

        return MatchingEngine.Evaluate(pool, criteria, referenceDate);
    }
}
