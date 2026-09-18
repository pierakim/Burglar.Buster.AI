using System.ClientModel;
using BurglarBuster.Core.Tools;
using OpenAI;
using OpenAI.Chat;
using CoreChatMessage = BurglarBuster.Core.Agent.ChatMessage;
using CoreChatRole = BurglarBuster.Core.Agent.ChatRole;

namespace BurglarBuster.Infrastructure.Agent;

/// <summary>
/// The one real implementation of <see cref="BurglarBuster.Core.Agent.IChatCompletionClient" />,
/// wrapping the official OpenAI .NET SDK pointed at an Azure OpenAI resource's
/// OpenAI-compatible "v1" endpoint (see DECISIONS.md for why this package rather than
/// Azure.AI.OpenAI). All SDK-specific types stay inside this one file.
/// </summary>
public sealed class AzureOpenAIChatCompletionClient : BurglarBuster.Core.Agent.IChatCompletionClient
{
    private readonly ChatClient _chatClient;
    private readonly string _deploymentName;

    public AzureOpenAIChatCompletionClient(string baseUrl, string apiKey, string deploymentName)
    {
        _deploymentName = deploymentName;
        var options = new OpenAIClientOptions { Endpoint = new Uri(baseUrl) };
        _chatClient = new ChatClient(deploymentName, new ApiKeyCredential(apiKey), options);
    }

    /// <summary>
    /// This method is the only one that actually calls the OpenAI SDK. 
    /// It converts the BurglarBuster.Core.Agent types to the SDK types, calls the SDK, and converts the result back to BurglarBuster.Core.Agent types.
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="tools"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<BurglarBuster.Core.Agent.ChatCompletionResult> CompleteAsync(
        IReadOnlyList<CoreChatMessage> messages,
        IReadOnlyList<ToolDefinition> tools,
        CancellationToken cancellationToken)
    {
        var sdkMessages = messages.Select(ToSdkMessage).ToList();

        var options = new ChatCompletionOptions();

        foreach (var tool in tools)
        {
            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: tool.Name,
                functionDescription: tool.Description,
                functionParameters: BinaryData.FromString(tool.ParametersSchemaJson)));
        }

        ChatCompletion completion = await _chatClient.CompleteChatAsync(sdkMessages, options, cancellationToken);

        if (completion.FinishReason == ChatFinishReason.ToolCalls)
        {
            var toolCalls = completion.ToolCalls.Select(tc => new BurglarBuster.Core.Agent.RequestedToolCall
            {
                Id = tc.Id,
                ToolName = tc.FunctionName,
                ArgumentsJson = tc.FunctionArguments.ToString(),
            }).ToList();

            return new BurglarBuster.Core.Agent.ChatCompletionResult
            {
                ToolCalls = toolCalls,
                Content = null,
                ModelId = _deploymentName,
                PromptTokens = completion.Usage?.InputTokenCount,
                CompletionTokens = completion.Usage?.OutputTokenCount,
            };
        }

        var text = completion.Content.Count > 0 ? completion.Content[0].Text : null;

        return new BurglarBuster.Core.Agent.ChatCompletionResult
        {
            ToolCalls = [],
            Content = text,
            ModelId = _deploymentName,
            PromptTokens = completion.Usage?.InputTokenCount,
            CompletionTokens = completion.Usage?.OutputTokenCount,
        };
    }

    private static OpenAI.Chat.ChatMessage ToSdkMessage(CoreChatMessage message) => message.Role switch
    {
        CoreChatRole.System => ChatMessage.CreateSystemMessage(message.Content),
        CoreChatRole.User => ChatMessage.CreateUserMessage(message.Content),
        CoreChatRole.Assistant => BuildAssistantMessage(message),
        CoreChatRole.Tool => ChatMessage.CreateToolMessage(message.ToolCallId, message.Content),
        _ => throw new NotSupportedException($"Unsupported chat role: {message.Role}"),
    };

    private static OpenAI.Chat.ChatMessage BuildAssistantMessage(CoreChatMessage message)
    {
        if (message.ToolCalls is { Count: > 0 })
        {
            var toolCalls = message.ToolCalls
                .Select(tc => ChatToolCall.CreateFunctionToolCall(tc.Id, tc.ToolName, BinaryData.FromString(tc.ArgumentsJson)))
                .ToList();
            return ChatMessage.CreateAssistantMessage(toolCalls);
        }

        return ChatMessage.CreateAssistantMessage(message.Content);
    }
}
