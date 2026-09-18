using BurglarBuster.Core.Tools;

namespace BurglarBuster.Infrastructure.Tools;

/// <summary>
/// The allowlist itself: built once from the injected <see cref="ITool" />
/// implementations (registered in DI, see Program.cs), keyed by name. A tool name
/// that isn't in this dictionary can never be executed, no matter what the model asks
/// for (spec §10, §11's "unknown tool rejection").
/// </summary>
public sealed class ToolDispatcher : IToolDispatcher
{
    private readonly IReadOnlyDictionary<string, ITool> _tools;

    public ToolDispatcher(IEnumerable<ITool> tools)
    {
        _tools = tools.ToDictionary(t => t.Name, StringComparer.Ordinal);
    }

    public Task<ToolDispatchResult> DispatchAsync(string toolName, string argumentsJson, CancellationToken cancellationToken)
    {
        if (!_tools.TryGetValue(toolName, out var tool))
        {
            return Task.FromResult(ToolDispatchResult.UnknownTool(toolName));
        }

        return tool.ExecuteAsync(argumentsJson, cancellationToken);
    }
}
