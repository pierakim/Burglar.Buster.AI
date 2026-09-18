using System.Text.Json;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Repositories;
using BurglarBuster.Infrastructure.Seeding;
using BurglarBuster.Infrastructure.Tools;
using BurglarBuster.Tests.TestSupport;

namespace BurglarBuster.Tests.Tools;

public class CompareCandidatesToolTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();
    private readonly CompareCandidatesTool _tool;

    public CompareCandidatesToolTests()
    {
        _tool = new CompareCandidatesTool(new PersonReadRepository(_database.Context), TimeProvider.System);
    }

    [Fact]
    public async Task Ranks_the_requested_candidates_against_the_criteria()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var result = await _tool.ExecuteAsync(
            """{"criteria":{"familyName":"Ellison","approximateAge":40},"personIds":["BB-0010","BB-0011"]}""",
            CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.Success, result.Status);
        var body = JsonSerializer.Deserialize<CompareCandidatesResult>(result.ResultJson!, new JsonSerializerOptions(JsonSerializerOptions.Web))!;
        Assert.Equal(2, body.Candidates.Count);
        Assert.Empty(body.UnknownPersonIds);
        Assert.All(body.Candidates, c => Assert.True(c.Score > 0));
    }

    [Fact]
    public async Task Unknown_ids_are_reported_separately_not_silently_dropped()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var result = await _tool.ExecuteAsync(
            """{"criteria":{"familyName":"Miller"},"personIds":["BB-0001","BB-9999"]}""",
            CancellationToken.None);

        var body = JsonSerializer.Deserialize<CompareCandidatesResult>(result.ResultJson!, new JsonSerializerOptions(JsonSerializerOptions.Web))!;
        Assert.Single(body.Candidates);
        Assert.Equal("BB-0001", body.Candidates[0].PersonId);
        Assert.Contains("BB-9999", body.UnknownPersonIds);
    }

    [Fact]
    public async Task Too_many_ids_is_an_invalid_argument()
    {
        var result = await _tool.ExecuteAsync(
            """{"criteria":{"familyName":"Miller"},"personIds":["BB-0001","BB-0002","BB-0003","BB-0004"]}""",
            CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.InvalidArguments, result.Status);
    }

    [Fact]
    public async Task Empty_id_list_is_an_invalid_argument()
    {
        var result = await _tool.ExecuteAsync(
            """{"criteria":{"familyName":"Miller"},"personIds":[]}""",
            CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.InvalidArguments, result.Status);
    }

    [Fact]
    public async Task Malformed_person_id_in_the_list_is_an_invalid_argument()
    {
        var result = await _tool.ExecuteAsync(
            """{"criteria":{"familyName":"Miller"},"personIds":["not-an-id"]}""",
            CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.InvalidArguments, result.Status);
    }

    public void Dispose() => _database.Dispose();
}
