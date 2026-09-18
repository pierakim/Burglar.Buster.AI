using System.Text.Json.Serialization;

namespace BurglarBuster.Core.Tools;

[JsonConverter(typeof(JsonStringEnumConverter<ToolDispatchStatus>))]
public enum ToolDispatchStatus
{
    [JsonStringEnumMemberName("success")]
    Success,

    [JsonStringEnumMemberName("unknownTool")]
    UnknownTool,

    [JsonStringEnumMemberName("invalidArguments")]
    InvalidArguments,

    [JsonStringEnumMemberName("notFound")]
    NotFound,
}

/// <summary>
/// The uniform outcome of any tool call, regardless of which tool ran. This is what
/// the (Milestone 5) agent loop will feed back to the model as a tool result — never a
/// raw exception, connection string, or other internal detail (spec §10, §17).
/// </summary>
public sealed record ToolDispatchResult
{
    public required ToolDispatchStatus Status { get; init; }

    /// <summary>The tool's typed result, already serialized to JSON. Only set on Success.</summary>
    public string? ResultJson { get; init; }

    /// <summary>A safe, human-readable explanation. Set on every non-Success status.</summary>
    public string? ErrorMessage { get; init; }

    public static ToolDispatchResult Success(string resultJson) => new()
    {
        Status = ToolDispatchStatus.Success,
        ResultJson = resultJson,
    };

    public static ToolDispatchResult UnknownTool(string toolName) => new()
    {
        Status = ToolDispatchStatus.UnknownTool,
        ErrorMessage = $"Unknown tool \"{toolName}\".",
    };

    public static ToolDispatchResult InvalidArguments(string message) => new()
    {
        Status = ToolDispatchStatus.InvalidArguments,
        ErrorMessage = message,
    };

    public static ToolDispatchResult NotFound(string message) => new()
    {
        Status = ToolDispatchStatus.NotFound,
        ErrorMessage = message,
    };
}
