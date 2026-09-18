namespace BurglarBuster.Core.Agent;

public enum ChatRole
{
    System,
    User,
    Assistant,
    Tool,
}

/// <summary>
/// One message in the conversation sent to the model. Provider-agnostic — translated
/// to/from whatever the actual Foundry/Azure OpenAI SDK types are entirely inside
/// Infrastructure, so the loop and its tests never depend on the SDK directly.
/// </summary>
public sealed record ChatMessage
{
    public required ChatRole Role { get; init; }

    /// <summary>Text content. Null for an Assistant message that only requested tool calls.</summary>
    public string? Content { get; init; }

    /// <summary>Set only on Assistant messages that requested one or more tool calls.</summary>
    public IReadOnlyList<RequestedToolCall>? ToolCalls { get; init; }

    /// <summary>Set only on Tool-role messages: which requested call this is the result of.</summary>
    public string? ToolCallId { get; init; }

    public static ChatMessage FromSystem(string content) => new() { Role = ChatRole.System, Content = content };

    public static ChatMessage FromUser(string content) => new() { Role = ChatRole.User, Content = content };

    public static ChatMessage FromToolResult(string toolCallId, string content) => new()
    {
        Role = ChatRole.Tool,
        ToolCallId = toolCallId,
        Content = content,
    };
}

public sealed record RequestedToolCall
{
    public required string Id { get; init; }

    public required string ToolName { get; init; }

    public required string ArgumentsJson { get; init; }
}
