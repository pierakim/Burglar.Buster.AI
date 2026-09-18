namespace BurglarBuster.Core.Agent;

/// <summary>One model turn's outcome: either tool calls to execute, or a final text reply.</summary>
public sealed record ChatCompletionResult
{
    public required IReadOnlyList<RequestedToolCall> ToolCalls { get; init; }

    /// <summary>The model's text reply. Non-null exactly when ToolCalls is empty.</summary>
    public string? Content { get; init; }

    /// <summary>The model/deployment identifier actually used, for logging (spec §16). Never logged with secrets.</summary>
    public string? ModelId { get; init; }

    public int? PromptTokens { get; init; }

    public int? CompletionTokens { get; init; }
}
