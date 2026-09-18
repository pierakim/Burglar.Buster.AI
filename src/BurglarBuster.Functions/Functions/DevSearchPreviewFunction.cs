using System.Net;
using BurglarBuster.Core.Matching;
using BurglarBuster.Infrastructure.Matching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BurglarBuster.Functions.Functions;

/// <summary>
/// TEMPORARY development-only endpoint for exercising the Milestone 3 matching engine
/// directly via Swagger UI before the agent loop/tool layer exist. Explicitly permitted
/// by BURGLAR_BUSTER_PROJECT_SPEC.md §20 Milestone 3 ("a temporary direct test harness
/// or internal endpoint if useful, clearly marked for development") — tracked for
/// removal in Milestone 9 ("Remove temporary development endpoints if no longer
/// useful"). Not part of the public API contract; never the real /api/investigations
/// endpoint, which requires the agent loop (Milestone 5).
/// </summary>
public sealed class DevSearchPreviewFunction(MatchingService matchingService, TimeProvider timeProvider)
{
    [Function("DevSearchPreview")]
    [OpenApiOperation("DevSearchPreview", "Development (temporary)", Summary = "[DEV ONLY] Preview the matching engine", Description = "Temporary development-only endpoint. Runs the deterministic matching engine directly against structured search criteria, bypassing the (not-yet-built) agent loop. Removed once Milestone 9 cleanup happens.")]
    [OpenApiRequestBody("application/json", typeof(SearchCriteria), Description = "Structured search criteria — the same shape the model will eventually construct via the search_people tool.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(MatchResult), Description = "The deterministic outcome and ranked candidates.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(string), Description = "The request body wasn't valid JSON.")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "dev/search-preview")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        SearchCriteria? criteria;
        try
        {
            criteria = await request.ReadFromJsonAsync<SearchCriteria>(cancellationToken);
        }
        catch (System.Text.Json.JsonException)
        {
            return new BadRequestObjectResult("Request body must be valid JSON matching SearchCriteria.");
        }

        if (criteria is null)
        {
            return new BadRequestObjectResult("Request body must be valid JSON matching SearchCriteria.");
        }

        var referenceDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var result = await matchingService.SearchAsync(criteria, referenceDate, cancellationToken);
        return new OkObjectResult(result);
    }
}
