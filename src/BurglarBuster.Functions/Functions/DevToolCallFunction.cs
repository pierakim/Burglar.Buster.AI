using System.Net;
using BurglarBuster.Core.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BurglarBuster.Functions.Functions;

/// <summary>
/// TEMPORARY development-only endpoint for exercising the Milestone 4 tool layer
/// directly via Swagger UI before the agent loop exists — same rationale and removal
/// plan as DevSearchPreviewFunction (spec §20 Milestone 3/4, §9 Milestone 9 cleanup).
/// Not part of the public API contract. Runs tool calls exactly the way the
/// (Milestone 5) agent loop eventually will: by name through the allowlisted
/// IToolDispatcher, never anything else.
/// </summary>
public sealed class DevToolCallFunction(IToolDispatcher dispatcher)
{
    [Function("DevToolCall")]
    [OpenApiOperation("DevToolCall", "Development (temporary)", Summary = "[DEV ONLY] Call a tool directly", Description = "Temporary development-only endpoint. Dispatches one tool call by name through the same allowlisted IToolDispatcher the agent loop will use. toolName must be one of: search_people, get_person_record, get_aliases, get_address_history, get_physical_description, get_case_references, compare_candidates. argumentsJson is that tool's arguments, JSON-encoded as a string.")]
    [OpenApiRequestBody("application/json", typeof(ToolCallRequest), Description = "The tool name and its JSON-encoded arguments.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ToolDispatchResult), Description = "The dispatch result — check \"status\" (success/unknownTool/invalidArguments/notFound).")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(string), Description = "The request body wasn't valid JSON.")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "dev/tool-call")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        ToolCallRequest? callRequest;
        try
        {
            callRequest = await request.ReadFromJsonAsync<ToolCallRequest>(cancellationToken);
        }
        catch (System.Text.Json.JsonException)
        {
            return new BadRequestObjectResult("Request body must be valid JSON with \"toolName\" and \"argumentsJson\" fields.");
        }

        if (callRequest is null || string.IsNullOrWhiteSpace(callRequest.ToolName))
        {
            return new BadRequestObjectResult("Request body must be valid JSON with \"toolName\" and \"argumentsJson\" fields.");
        }

        var result = await dispatcher.DispatchAsync(callRequest.ToolName, callRequest.ArgumentsJson ?? "{}", cancellationToken);
        return new OkObjectResult(result);
    }
}

public sealed record ToolCallRequest(string ToolName, string? ArgumentsJson);
