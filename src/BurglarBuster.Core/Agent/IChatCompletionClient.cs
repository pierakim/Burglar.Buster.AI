using BurglarBuster.Core.Tools;

namespace BurglarBuster.Core.Agent;

/// <summary>
/// The only way the loop talks to a model. Normal tests use a fake implementation of
/// this interface — never a real model (spec §18). The one real implementation
/// (Infrastructure) wraps whatever Foundry/Azure OpenAI SDK is actually configured.
/// </summary>
public interface IChatCompletionClient
{
    Task<ChatCompletionResult> CompleteAsync(
        IReadOnlyList<ChatMessage> messages,
        IReadOnlyList<ToolDefinition> tools,
        CancellationToken cancellationToken);
}
