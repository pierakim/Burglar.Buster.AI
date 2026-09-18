using System.Text.Json;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Repositories;
using BurglarBuster.Infrastructure.Seeding;
using BurglarBuster.Infrastructure.Tools;
using BurglarBuster.Tests.TestSupport;

namespace BurglarBuster.Tests.Tools;

public class GetCaseReferencesToolTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();
    private readonly GetCaseReferencesTool _tool;

    public GetCaseReferencesToolTests()
    {
        _tool = new GetCaseReferencesTool(new PersonReadRepository(_database.Context));
    }

    [Fact]
    public async Task Person_with_a_case_reference_returns_it()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var result = await _tool.ExecuteAsync("""{"personId":"BB-0001"}""", CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.Success, result.Status);
        var body = JsonSerializer.Deserialize<GetCaseReferencesResult>(result.ResultJson!, new JsonSerializerOptions(JsonSerializerOptions.Web))!;
        var reference = Assert.Single(body.CaseReferences);
        Assert.Equal("Closed", reference.Status);
    }

    public void Dispose() => _database.Dispose();
}
