using System.Text.Json;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Repositories;
using BurglarBuster.Infrastructure.Seeding;
using BurglarBuster.Infrastructure.Tools;
using BurglarBuster.Tests.TestSupport;

namespace BurglarBuster.Tests.Tools;

public class GetAliasesToolTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();
    private readonly GetAliasesTool _tool;

    public GetAliasesToolTests()
    {
        _tool = new GetAliasesTool(new PersonReadRepository(_database.Context));
    }

    [Fact]
    public async Task Person_with_an_alias_returns_it()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var result = await _tool.ExecuteAsync("""{"personId":"BB-0005"}""", CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.Success, result.Status);
        var body = JsonSerializer.Deserialize<GetAliasesResult>(result.ResultJson!, new JsonSerializerOptions(JsonSerializerOptions.Web))!;
        var alias = Assert.Single(body.Aliases);
        Assert.Equal("Bobby Nguyen", alias.FullName);
    }

    [Fact]
    public async Task Person_with_no_alias_returns_an_empty_list()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var result = await _tool.ExecuteAsync("""{"personId":"BB-0001"}""", CancellationToken.None);

        var body = JsonSerializer.Deserialize<GetAliasesResult>(result.ResultJson!, new JsonSerializerOptions(JsonSerializerOptions.Web))!;
        Assert.Empty(body.Aliases);
    }

    [Fact]
    public async Task Unknown_person_id_is_not_found()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var result = await _tool.ExecuteAsync("""{"personId":"BB-9999"}""", CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.NotFound, result.Status);
    }

    public void Dispose() => _database.Dispose();
}
