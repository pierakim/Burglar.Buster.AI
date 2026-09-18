using System.Text.Json;
using BurglarBuster.Core.Matching;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Repositories;

namespace BurglarBuster.Infrastructure.Tools;

public sealed class CompareCandidatesTool(IPersonReadRepository repository, TimeProvider timeProvider) : ITool
{
    public string Name => ToolNames.CompareCandidates;

    public async Task<ToolDispatchResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        CompareCandidatesArguments? args;
        try
        {
            args = JsonSerializer.Deserialize<CompareCandidatesArguments>(argumentsJson, ToolJson.Options);
        }
        catch (JsonException)
        {
            return ToolDispatchResult.InvalidArguments("Arguments must be a valid JSON object matching the compare_candidates schema.");
        }

        if (args is null)
        {
            return ToolDispatchResult.InvalidArguments("Arguments must be a valid JSON object matching the compare_candidates schema.");
        }

        if (args.PersonIds.Count == 0)
        {
            return ToolDispatchResult.InvalidArguments("\"personIds\" must contain at least one person ID.");
        }

        if (args.PersonIds.Count > MatchingPolicy.MaxDetailedCandidates)
        {
            return ToolDispatchResult.InvalidArguments($"\"personIds\" must contain at most {MatchingPolicy.MaxDetailedCandidates} person IDs.");
        }

        if (args.PersonIds.Any(id => !PersonIdValidator.IsValidFormat(id)))
        {
            return ToolDispatchResult.InvalidArguments("Every entry in \"personIds\" must match the format \"BB-####\".");
        }

        var people = await repository.GetByIdsAsync(args.PersonIds, cancellationToken);
        var foundIds = people.Select(p => p.PersonId).ToHashSet();
        var unknownIds = args.PersonIds.Where(id => !foundIds.Contains(id)).ToList();

        var referenceDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var ranked = people
            .Select(p => (Person: p, Result: CandidateScorer.Evaluate(p, args.Criteria, referenceDate)))
            .OrderByDescending(x => x.Result.Score)
            .ThenBy(x => x.Person.PersonId, StringComparer.Ordinal)
            .Select((x, index) => new ScoredCandidate
            {
                PersonId = x.Person.PersonId,
                Score = x.Result.Score,
                Rank = index + 1,
                Evidence = x.Result.Evidence,
            })
            .ToList();

        var result = new CompareCandidatesResult { Candidates = ranked, UnknownPersonIds = unknownIds };

        return ToolDispatchResult.Success(JsonSerializer.Serialize(result, ToolJson.Options));
    }
}
