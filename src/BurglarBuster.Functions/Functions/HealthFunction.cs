using System.Net;
using BurglarBuster.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BurglarBuster.Functions.Functions;

/// <summary>
/// Liveness/readiness probe: confirms the host is up, configuration bound
/// successfully at startup (see AgentLoopOptions/DatabaseOptions + ValidateOnStart in
/// Program.cs), and the database is reachable. Never leaks connection details.
/// </summary>
public sealed class HealthFunction(BurglarBusterDbContext dbContext)
{
    [Function("Health")]
    [OpenApiOperation("Health", "System", Summary = "Application health check", Description = "Confirms the Function host is running, configuration bound successfully at startup, and the database is reachable.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(HealthResponse), Description = "The application is healthy.")]
    [OpenApiResponseWithBody(HttpStatusCode.ServiceUnavailable, "application/json", typeof(HealthResponse), Description = "The database is unreachable.")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        var databaseReachable = await dbContext.Database.CanConnectAsync(cancellationToken);

        var response = new HealthResponse(
            databaseReachable ? "healthy" : "unhealthy",
            DateTimeOffset.UtcNow,
            databaseReachable ? "connected" : "unavailable");

        return databaseReachable
            ? new OkObjectResult(response)
            : new ObjectResult(response) { StatusCode = (int)HttpStatusCode.ServiceUnavailable };
    }
}

public sealed record HealthResponse(string Status, DateTimeOffset TimestampUtc, string Database);
