using System.Net;
using BurglarBuster.Core.Agent;
using BurglarBuster.Functions.Contracts;
using BurglarBuster.Infrastructure.Agent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BurglarBuster.Functions.Functions;

/// <summary>
/// POST /api/investigations — the real public HTTP contract from spec §13. Runs the
/// full manual agent loop (Milestone 5): builds the conversation, lets the model call
/// tools, and returns the deterministic result once the model concludes or a bound is
/// hit. Always persists, regardless of how the loop ended.
/// </summary>
public sealed class InvestigationsFunction(InvestigationAgentLoop agentLoop)
{
    private const int MaxDescriptionLength = 4000;

    [Function("CreateInvestigation")]
    [OpenApiOperation("CreateInvestigation", "Investigations", Summary = "Start an investigation", Description = "Interprets a free-text operator description, lets the model investigate using the approved tools, and returns a structured, persisted result. All data is synthetic; the result is a candidate for human verification, never a confirmed identity.")]
    [OpenApiRequestBody("application/json", typeof(InvestigationHttpRequest), Description = "The operator's free-text description of the person encountered.")]
    [OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof(InvestigationResponse), Description = "The completed, persisted investigation.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(ProblemDetails), Description = "The description was missing, blank, or too long.")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "investigations")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        InvestigationHttpRequest? httpRequest;
        try
        {
            httpRequest = await request.ReadFromJsonAsync<InvestigationHttpRequest>(cancellationToken);
        }
        catch (System.Text.Json.JsonException)
        {
            return ValidationProblem("Request body must be valid JSON with a \"description\" string field.");
        }

        var description = httpRequest?.Description?.Trim();

        if (string.IsNullOrEmpty(description))
        {
            return ValidationProblem("\"description\" is required and cannot be blank.");
        }

        if (description.Length > MaxDescriptionLength)
        {
            return ValidationProblem($"\"description\" must be at most {MaxDescriptionLength} characters.");
        }

        var result = await agentLoop.RunAsync(new InvestigationRequest { Description = description }, cancellationToken);
        var response = InvestigationResponse.FromResult(result);

        return new ObjectResult(response)
        {
            StatusCode = (int)HttpStatusCode.Created,
        };
    }

    private static BadRequestObjectResult ValidationProblem(string detail) => new(new ProblemDetails
    {
        Title = "Invalid investigation request",
        Status = (int)HttpStatusCode.BadRequest,
        Detail = detail,
    });
}
