namespace BurglarBuster.Core.Tools;

/// <summary>
/// One allowed tool. Implementations own their own argument deserialization and
/// validation — the dispatcher only owns the allowlist (spec §10: "be callable only
/// through an allowlist maintained by the host").
/// </summary>
public interface ITool
{
    /// <summary>Must be one of the constants in <see cref="ToolNames" />.</summary>
    string Name { get; }

    Task<ToolDispatchResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken);
}
