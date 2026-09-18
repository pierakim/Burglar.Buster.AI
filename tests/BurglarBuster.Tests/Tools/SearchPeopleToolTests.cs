using System.Text.Json;
using BurglarBuster.Core.Matching;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Matching;
using BurglarBuster.Infrastructure.Seeding;
using BurglarBuster.Infrastructure.Tools;
using BurglarBuster.Tests.TestSupport;

namespace BurglarBuster.Tests.Tools;

public class SearchPeopleToolTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();
    private readonly SearchPeopleTool _tool;

    public SearchPeopleToolTests()
    {
        var matchingService = new MatchingService(new PersonCandidateRepository(_database.Context));
        _tool = new SearchPeopleTool(matchingService, TimeProvider.System);
    }

    [Fact]
    public async Task Exact_details_find_the_unique_match()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var result = await _tool.ExecuteAsync("""{"givenName":"Daniel","familyName":"Miller","dateOfBirth":"1989-04-12"}""", CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.Success, result.Status);
        var body = JsonSerializer.Deserialize<MatchResult>(result.ResultJson!, new JsonSerializerOptions(JsonSerializerOptions.Web))!;
        Assert.Equal(MatchOutcome.StrongCandidate, body.Outcome);
        Assert.Equal("BB-0001", body.Candidates[0].PersonId);
    }

    [Fact]
    public async Task Malformed_json_is_an_invalid_argument()
    {
        var result = await _tool.ExecuteAsync("not json", CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.InvalidArguments, result.Status);
    }

    [Fact]
    public async Task Empty_criteria_object_is_a_valid_call_that_yields_insufficient_information()
    {
        var result = await _tool.ExecuteAsync("{}", CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.Success, result.Status);
        var body = JsonSerializer.Deserialize<MatchResult>(result.ResultJson!, new JsonSerializerOptions(JsonSerializerOptions.Web))!;
        Assert.Equal(MatchOutcome.InsufficientInformation, body.Outcome);
    }

    public void Dispose() => _database.Dispose();
}
