using BurglarBuster.Core.Entities;

namespace BurglarBuster.Core.Matching;

/// <summary>
/// Stage 2 of the two-stage design from spec §9: given a bounded candidate pool
/// (already retrieved by the repository) and the original criteria, score every
/// candidate, rank them, and apply the deterministic outcome policy. Pure — no I/O —
/// so it's fully unit-testable without a database.
/// </summary>
public static class MatchingEngine
{
    public static MatchResult Evaluate(IReadOnlyList<Person> candidatePool, SearchCriteria criteria, DateOnly referenceDate)
    {
        if (!OutcomePolicy.IsSufficient(criteria))
        {
            return MatchResult.Empty(MatchOutcome.InsufficientInformation);
        }

        var scored = candidatePool
            .Select(p => (Person: p, Result: CandidateScorer.Evaluate(p, criteria, referenceDate)))
            .Where(x => x.Result.Score >= MatchingPolicy.PlausibleCandidateMinScore)
            .OrderByDescending(x => x.Result.Score)
            .ThenBy(x => x.Person.PersonId, StringComparer.Ordinal)
            .ToList();

        if (scored.Count == 0)
        {
            return MatchResult.Empty(MatchOutcome.NoMatch);
        }

        var top = scored.Take(MatchingPolicy.MaxCandidatesReturned).ToList();

        var candidates = top.Select((x, index) => new ScoredCandidate
        {
            PersonId = x.Person.PersonId,
            Score = x.Result.Score,
            Rank = index + 1,
            Evidence = x.Result.Evidence,
        }).ToList();

        // Pairwise-compare every candidate we're about to return against every other
        // one (not against the search criteria — that's already done above) to catch
        // "these two DB rows look like the same real person entered twice", e.g.
        // BB-0013/BB-0014. `j = i + 1` avoids comparing a pair twice or a candidate to
        // itself; `top` is capped at MaxCandidatesReturned (5), so this is at most 10
        // comparisons, not a performance concern. The first duplicate pair found wins
        // and short-circuits the rest of the outcome logic below — a potential
        // database data-quality issue takes priority over deciding how well the
        // criteria matched.
        for (var i = 0; i < top.Count; i++)
        {
            for (var j = i + 1; j < top.Count; j++)
            {
                if (DuplicateDetector.AreLikelyDuplicates(top[i].Person, top[j].Person))
                {
                    return new MatchResult { Outcome = MatchOutcome.PotentialDuplicate, Candidates = candidates };
                }
            }
        }

        var topCandidate = top[0];

        if (topCandidate.Result.Evidence.HasInternalInconsistency)
        {
            return new MatchResult { Outcome = MatchOutcome.ConflictingInformation, Candidates = candidates };
        }

        var secondScore = top.Count > 1 ? top[1].Result.Score : 0;
        var meetsStrongBar = topCandidate.Result.Score >= MatchingPolicy.StrongCandidateMinScore
            && topCandidate.Result.Evidence.IndependentGroupsMatched >= MatchingPolicy.MinIndependentGroupsForStrongMatch
            && (top.Count == 1 || topCandidate.Result.Score - secondScore >= MatchingPolicy.StrongCandidateMinMarginOverSecond);

        return new MatchResult
        {
            Outcome = meetsStrongBar ? MatchOutcome.StrongCandidate : MatchOutcome.Ambiguous,
            Candidates = candidates,
        };
    }
}
