namespace BurglarBuster.Core.Tools;

/// <summary>
/// The single entry point the (Milestone 5) agent loop calls for every tool the model
/// requests. Never executes anything not in <see cref="ToolNames.All" />.
/// </summary>
public interface IToolDispatcher
{
    Task<ToolDispatchResult> DispatchAsync(string toolName, string argumentsJson, CancellationToken cancellationToken);
}
