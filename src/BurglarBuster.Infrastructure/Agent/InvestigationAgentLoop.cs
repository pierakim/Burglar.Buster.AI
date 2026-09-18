using System.Text.Json;
using BurglarBuster.Core.Agent;
using BurglarBuster.Core.Entities;
using BurglarBuster.Core.Matching;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BurglarBuster.Infrastructure.Agent; 

/// <summary>
/// The manual agent loop from spec §11: build the conversation, send it plus the tool
/// catalog to the model, execute whatever tools it requests, feed results back, repeat
/// until it produces a final response or a limit is hit. The outcome and candidates in
/// the result ALWAYS come from the last successful search_people call tracked during
/// the loop — never from anything the model writes (see DECISIONS.md). Persists the
/// investigation on every path, clean or not.
/// </summary>
public sealed class InvestigationAgentLoop(
    IChatCompletionClient chatClient,
    IToolDispatcher toolDispatcher,
    IInvestigationRepository investigationRepository,
    TimeProvider timeProvider,
    IOptions<AgentLoopOptions> options,
    string modelDeploymentId,
    ILogger<InvestigationAgentLoop> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerOptions.Web);

    public async Task<InvestigationResult> RunAsync(InvestigationRequest request, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var investigationId = InvestigationIdGenerator.NewId(timeProvider);

        var messages = new List<ChatMessage>
        {
            ChatMessage.FromSystem(AgentInstructions.Text),
            ChatMessage.FromUser(request.Description),
        };

        var executedCallKeys = new HashSet<string>(StringComparer.Ordinal);
        MatchResult? lastMatchResult = null;
        var turnCount = 0;
        var toolCallCount = 0;

        using var overallCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        overallCts.CancelAfter(TimeSpan.FromSeconds(settings.OverallTimeoutSeconds));

        while (true)
        {
            if (turnCount >= settings.MaxTurns)
            {
                return await FinishAsync(
                    TerminationReason.MaxTurnsReached, lastMatchResult,
                    "The investigation reached its maximum number of reasoning turns before concluding.",
                    [], investigationId, request, turnCount, toolCallCount, cancellationToken);
            }

            turnCount++;
            logger.LogInformation("###################################");
            logger.LogInformation("###################################");
            logger.LogInformation(
                "Turn starting. Investigation={InvestigationId} Turn={Turn}/{MaxTurns} ToolCallsSoFar={ToolCallsSoFar}/{MaxToolCalls}",
                investigationId, turnCount, settings.MaxTurns, toolCallCount, settings.MaxToolCalls);

            ChatCompletionResult response;
            try
            {
                using var upstreamCts = CancellationTokenSource.CreateLinkedTokenSource(overallCts.Token);
                upstreamCts.CancelAfter(TimeSpan.FromSeconds(settings.UpstreamTimeoutSeconds));

                var callStart = timeProvider.GetTimestamp();
                response = await chatClient.CompleteAsync(messages, ToolCatalog.Definitions, upstreamCts.Token);
                logger.LogInformation(
                    "Model call complete. Investigation={InvestigationId} Deployment={Deployment} Turn={Turn} DurationMs={DurationMs} PromptTokens={PromptTokens} CompletionTokens={CompletionTokens}",
                    investigationId, modelDeploymentId, turnCount, timeProvider.GetElapsedTime(callStart).TotalMilliseconds,
                    response.PromptTokens, response.CompletionTokens);

                if (response.ToolCalls.Count > 0)
                {
                    logger.LogInformation(
                        "Model decision: requested {ToolCallCount} tool call(s). Investigation={InvestigationId} Turn={Turn} Tools={Tools}",
                        response.ToolCalls.Count, investigationId, turnCount,
                        string.Join(", ", response.ToolCalls.Select(tc => tc.ToolName)));
                }
                else
                {
                    logger.LogInformation(
                        "Model decision: final response (no tool calls). Investigation={InvestigationId} Turn={Turn}",
                        investigationId, turnCount);
                }
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }

                // The original cancellationToken firing was already handled above, so
                // if overallCts is cancelled here it can only be its own CancelAfter
                // timer; otherwise it must have been the per-call upstreamCts timer.
                var reason = overallCts.IsCancellationRequested
                    ? TerminationReason.OverallTimeout
                    : TerminationReason.UpstreamTimeout;

                logger.LogWarning("Model call timed out. Investigation={InvestigationId} Reason={Reason}", investigationId, reason);

                return await FinishAsync(
                    reason, lastMatchResult,
                    "The investigation timed out while waiting for the model.",
                    [], investigationId, request, turnCount, toolCallCount, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Upstream model call failed. Investigation={InvestigationId}", investigationId);

                return await FinishAsync(
                    TerminationReason.UpstreamError, lastMatchResult,
                    "The investigation could not continue because the model service reported an error.",
                    [], investigationId, request, turnCount, toolCallCount, cancellationToken);
            }

            if (response.ToolCalls.Count > 0)
            {
                var assistantToolCalls = new List<RequestedToolCall>();
                var toolResultMessages = new List<ChatMessage>();
                var stoppedForLimit = false;

                foreach (var toolCall in response.ToolCalls)
                {
                    if (toolCallCount >= settings.MaxToolCalls)
                    {
                        stoppedForLimit = true;
                        break;
                    }

                    var callKey = $"{toolCall.ToolName} {toolCall.ArgumentsJson}";
                    if (!executedCallKeys.Add(callKey))
                    {
                        logger.LogWarning("Repeated identical tool call detected. Investigation={InvestigationId} Tool={Tool}", investigationId, toolCall.ToolName);
                        return await FinishAsync(
                            TerminationReason.RepeatedToolCall, lastMatchResult,
                            "The investigation stopped because the same tool call was requested more than once with identical arguments.",
                            [], investigationId, request, turnCount, toolCallCount, cancellationToken);
                    }

                    var toolStart = timeProvider.GetTimestamp();

                    var dispatchResult = await toolDispatcher.DispatchAsync(toolCall.ToolName, toolCall.ArgumentsJson, cancellationToken);

                    logger.LogInformation(
                        "Tool call complete. Investigation={InvestigationId} Turn={Turn} Tool={Tool} Arguments={Arguments} Status={Status} DurationMs={DurationMs}",
                        investigationId, turnCount, toolCall.ToolName, Truncate(toolCall.ArgumentsJson, 500),
                        dispatchResult.Status, timeProvider.GetElapsedTime(toolStart).TotalMilliseconds);

                    if (dispatchResult.Status == ToolDispatchStatus.Success && toolCall.ToolName == ToolNames.SearchPeople)
                    {
                        lastMatchResult = JsonSerializer.Deserialize<MatchResult>(dispatchResult.ResultJson!, JsonOptions);
                        logger.LogInformation(
                            "search_people evaluated. Investigation={InvestigationId} Turn={Turn} Outcome={Outcome} CandidateCount={CandidateCount} TopCandidate={TopCandidate}",
                            investigationId, turnCount, lastMatchResult?.Outcome,
                            lastMatchResult?.Candidates.Count ?? 0,
                            lastMatchResult?.Candidates.Count > 0 ? lastMatchResult.Candidates[0].PersonId : "(none)");
                    }

                    toolCallCount++;
                    assistantToolCalls.Add(toolCall);
                    toolResultMessages.Add(ChatMessage.FromToolResult(toolCall.Id, BuildToolResultPayload(dispatchResult)));
                }

                if (assistantToolCalls.Count > 0)
                {
                    messages.Add(new ChatMessage { Role = ChatRole.Assistant, ToolCalls = assistantToolCalls });
                    messages.AddRange(toolResultMessages);
                }

                if (stoppedForLimit)
                {
                    return await FinishAsync(
                        TerminationReason.MaxToolCallsReached, lastMatchResult,
                        "The investigation reached its maximum number of tool calls before the model produced a final summary.",
                        [], investigationId, request, turnCount, toolCallCount, cancellationToken);
                }

                continue;
            }

            if (string.IsNullOrWhiteSpace(response.Content))
            {
                logger.LogWarning("Model returned neither tool calls nor content. Investigation={InvestigationId}", investigationId);
                return await FinishAsync(
                    TerminationReason.EmptyModelResponse, lastMatchResult,
                    "The investigation stopped because the model did not return a usable response.",
                    [], investigationId, request, turnCount, toolCallCount, cancellationToken);
            }

            var proposed = TryParseFinalResponse(response.Content);
            if (proposed is null)
            {
                logger.LogWarning("Model's final response did not match the required schema. Investigation={InvestigationId}", investigationId);
                return await FinishAsync(
                    TerminationReason.InvalidFinalResponse, lastMatchResult,
                    "The investigation stopped because the model's final response was not in the required format.",
                    [], investigationId, request, turnCount, toolCallCount, cancellationToken);
            }

            logger.LogInformation(
                "Model final response parsed. Investigation={InvestigationId} Turn={Turn} Summary={Summary} RequestedInformationCount={RequestedInformationCount}",
                investigationId, turnCount, proposed.Summary, proposed.RequestedInformation?.Count ?? 0);

            return await FinishAsync(
                TerminationReason.FinalResponse, lastMatchResult,
                proposed.Summary, proposed.RequestedInformation ?? [],
                investigationId, request, turnCount, toolCallCount, cancellationToken);
        }
    }

    private async Task<InvestigationResult> FinishAsync(
        TerminationReason reason,
        MatchResult? lastMatchResult,
        string summary,
        IReadOnlyList<string> requestedInformation,
        string investigationId,
        InvestigationRequest request,
        int turnCount,
        int toolCallCount,
        CancellationToken cancellationToken)
    {
        var outcome = lastMatchResult?.Outcome ?? MatchOutcome.ManualReviewRequired;
        var candidates = lastMatchResult?.Candidates ?? [];

        var result = new InvestigationResult
        {
            InvestigationId = investigationId,
            Outcome = outcome,
            Summary = summary,
            Candidates = candidates,
            RequestedInformation = requestedInformation,
            RequiresHumanReview = OutcomePolicy.RequiresHumanReview(outcome),
            TerminationReason = reason,
            ModelDeploymentId = modelDeploymentId,
            ModelTurnCount = turnCount,
            ToolCallCount = toolCallCount,
        };

        logger.LogInformation(
            "Investigation concluded. Investigation={InvestigationId} Outcome={Outcome} TerminationReason={TerminationReason} Turns={Turns} ToolCalls={ToolCalls} CandidateCount={CandidateCount}",
            investigationId, outcome, reason, turnCount, toolCallCount, candidates.Count);

        var entity = new Investigation
        {
            InvestigationId = result.InvestigationId,
            Description = request.Description,
            CreatedAtUtc = timeProvider.GetUtcNow(),
            Outcome = result.Outcome,
            Summary = result.Summary,
            RequiresHumanReview = result.RequiresHumanReview,
            ModelDeploymentId = result.ModelDeploymentId,
            ModelTurnCount = result.ModelTurnCount,
            ToolCallCount = result.ToolCallCount,
            TerminationReason = result.TerminationReason,
            RequestedInformationJson = JsonSerializer.Serialize(result.RequestedInformation, JsonOptions),
            Candidates = result.Candidates.Select(c => new InvestigationCandidate
            {
                InvestigationId = result.InvestigationId,
                PersonId = c.PersonId,
                Score = c.Score,
                Rank = c.Rank,
                MatchedEvidenceJson = JsonSerializer.Serialize(c.Evidence.MatchedEvidence, JsonOptions),
                ConflictsJson = JsonSerializer.Serialize(c.Evidence.Conflicts, JsonOptions),
                MissingEvidenceJson = JsonSerializer.Serialize(c.Evidence.MissingEvidence, JsonOptions),
            }).ToList(),
        };

        await investigationRepository.SaveAsync(entity, cancellationToken);

        return result;
    }

    private static string BuildToolResultPayload(ToolDispatchResult dispatchResult) =>
        dispatchResult.Status == ToolDispatchStatus.Success
            ? dispatchResult.ResultJson!
            : JsonSerializer.Serialize(dispatchResult, JsonOptions);

    private static ProposedFinalResponse? TryParseFinalResponse(string content)
    {
        try
        {
            return JsonSerializer.Deserialize<ProposedFinalResponse>(ExtractJson(content), JsonOptions) switch
            {
                { Summary.Length: > 0 } parsed => parsed,
                _ => null,
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Strips a markdown code fence if the model wrapped its JSON in one
    /// despite instructions not to — defensive, not a relaxation of validation.</summary>
    private static string ExtractJson(string content)
    {
        var trimmed = content.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var firstNewline = trimmed.IndexOf('\n');
        var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        return firstNewline > 0 && lastFence > firstNewline
            ? trimmed[(firstNewline + 1)..lastFence].Trim()
            : trimmed;
    }

    /// <summary>Defensive cap on logged tool arguments — normal criteria/IDs are far
    /// shorter than this, but nothing stops a future tool from accepting more.</summary>
    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : string.Concat(value.AsSpan(0, maxLength), "…(truncated)");
}
