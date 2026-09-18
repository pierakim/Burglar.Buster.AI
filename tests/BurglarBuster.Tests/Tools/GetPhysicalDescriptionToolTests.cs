using System.Text.Json;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Repositories;
using BurglarBuster.Infrastructure.Seeding;
using BurglarBuster.Infrastructure.Tools;
using BurglarBuster.Tests.TestSupport;

namespace BurglarBuster.Tests.Tools;

public class GetPhysicalDescriptionToolTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();
    private readonly GetPhysicalDescriptionTool _tool;

    public GetPhysicalDescriptionToolTests()
    {
        _tool = new GetPhysicalDescriptionTool(new PersonReadRepository(_database.Context));
    }

    [Fact]
    public async Task Multiple_observations_are_all_returned_oldest_first()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var result = await _tool.ExecuteAsync("""{"personId":"BB-0012"}""", CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.Success, result.Status);
        var body = JsonSerializer.Deserialize<GetPhysicalDescriptionResult>(result.ResultJson!, new JsonSerializerOptions(JsonSerializerOptions.Web))!;
        Assert.Equal(2, body.Observations.Count);
        Assert.Equal(170, body.Observations[0].ApproximateHeightCm);
        Assert.Equal(190, body.Observations[1].ApproximateHeightCm);
    }

    public void Dispose() => _database.Dispose();
}
