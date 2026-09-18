using BurglarBuster.Functions.Functions;
using BurglarBuster.Tests.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BurglarBuster.Tests.Functions;

public class HealthFunctionTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    [Fact]
    public async Task Run_returns_200_with_healthy_status_when_database_reachable()
    {
        var function = new HealthFunction(_database.Context);
        var request = new DefaultHttpContext().Request;

        var result = await function.Run(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<HealthResponse>(okResult.Value);
        Assert.Equal("healthy", body.Status);
        Assert.Equal("connected", body.Database);
        Assert.True(DateTimeOffset.UtcNow - body.TimestampUtc < TimeSpan.FromSeconds(5));
    }

    public void Dispose() => _database.Dispose();
}
