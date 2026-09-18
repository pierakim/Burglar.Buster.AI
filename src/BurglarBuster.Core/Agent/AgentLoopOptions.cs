using System.ComponentModel.DataAnnotations;

namespace BurglarBuster.Core.Agent;

/// <summary>
/// Bounds on the agent's autonomy: how many turns/tool calls it gets, how many
/// candidates it may investigate in detail, and how long it's allowed to run.
/// Values mirror the defaults in BURGLAR_BUSTER_PROJECT_SPEC.md §11. Lives in Core
/// (not just Functions) because the loop implementation itself needs these limits
/// directly, not only the config-binding layer.
/// </summary>
public sealed class AgentLoopOptions
{
    public const string SectionName = "Agent";

    [Range(1, 20)]
    public int MaxTurns { get; init; } = 6;

    [Range(1, 50)]
    public int MaxToolCalls { get; init; } = 8;

    [Range(1, 10)]
    public int MaxDetailedCandidates { get; init; } = 3;

    [Range(1, 300)]
    public int UpstreamTimeoutSeconds { get; init; } = 30;

    [Range(1, 600)]
    public int OverallTimeoutSeconds { get; init; } = 60;
}
