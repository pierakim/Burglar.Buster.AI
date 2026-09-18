using System.Text.Json.Serialization;

namespace BurglarBuster.Core.Agent;

/// <summary>Why the loop stopped — always recorded, success or not (spec §11, §16).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<TerminationReason>))]
public enum TerminationReason
{
    /// <summary>The model produced a valid final response and the loop concluded normally.</summary>
    [JsonStringEnumMemberName("final_response")]
    FinalResponse,

    [JsonStringEnumMemberName("max_turns_reached")]
    MaxTurnsReached,

    [JsonStringEnumMemberName("max_tool_calls_reached")]
    MaxToolCallsReached,

    [JsonStringEnumMemberName("overall_timeout")]
    OverallTimeout,

    [JsonStringEnumMemberName("upstream_timeout")]
    UpstreamTimeout,

    [JsonStringEnumMemberName("upstream_error")]
    UpstreamError,

    [JsonStringEnumMemberName("repeated_tool_call")]
    RepeatedToolCall,

    /// <summary>The model's final text wasn't valid JSON matching the required response shape.</summary>
    [JsonStringEnumMemberName("invalid_final_response")]
    InvalidFinalResponse,

    /// <summary>The model returned neither tool calls nor usable final content.</summary>
    [JsonStringEnumMemberName("empty_model_response")]
    EmptyModelResponse,

    [JsonStringEnumMemberName("cancelled")]
    Cancelled,
}
