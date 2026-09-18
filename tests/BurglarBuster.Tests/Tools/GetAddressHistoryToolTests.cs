using System.Text.Json;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Repositories;
using BurglarBuster.Infrastructure.Seeding;
using BurglarBuster.Infrastructure.Tools;
using BurglarBuster.Tests.TestSupport;

namespace BurglarBuster.Tests.Tools;

public class GetAddressHistoryToolTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();
    private readonly GetAddressHistoryTool _tool;

    public GetAddressHistoryToolTests()
    {
        _tool = new GetAddressHistoryTool(new PersonReadRepository(_database.Context));
    }

    [Fact]
    public async Task Previous_address_comes_before_current_address()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var result = await _tool.ExecuteAsync("""{"personId":"BB-0006"}""", CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.Success, result.Status);
        var body = JsonSerializer.Deserialize<GetAddressHistoryResult>(result.ResultJson!, new JsonSerializerOptions(JsonSerializerOptions.Web))!;
        Assert.Equal(2, body.Addresses.Count);
        Assert.NotNull(body.Addresses[0].ValidTo);
        Assert.Null(body.Addresses[1].ValidTo);
    }

    public void Dispose() => _database.Dispose();
}
