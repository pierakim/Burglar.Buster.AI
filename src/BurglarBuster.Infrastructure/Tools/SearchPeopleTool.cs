using System.Text.Json;
using BurglarBuster.Core.Matching;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Matching;

namespace BurglarBuster.Infrastructure.Tools;

public sealed class SearchPeopleTool(MatchingService matchingService, TimeProvider timeProvider) : ITool
{
    public string Name => ToolNames.SearchPeople;

    public async Task<ToolDispatchResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        SearchCriteria? criteria;
        try
        {
            criteria = JsonSerializer.Deserialize<SearchCriteria>(argumentsJson, ToolJson.Options);
        }
        catch (JsonException)
        {
            return ToolDispatchResult.InvalidArguments("Arguments must be a valid JSON object matching the search_people schema.");
        }

        if (criteria is null)
        {
            return ToolDispatchResult.InvalidArguments("Arguments must be a valid JSON object matching the search_people schema.");
        }

        var referenceDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var result = await matchingService.SearchAsync(criteria, referenceDate, cancellationToken);

        return ToolDispatchResult.Success(JsonSerializer.Serialize(result, ToolJson.Options));
    }
}
