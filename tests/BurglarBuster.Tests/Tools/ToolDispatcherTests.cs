using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Tools;

namespace BurglarBuster.Tests.Tools;

public class ToolDispatcherTests
{
    [Fact]
    public async Task Unknown_tool_name_is_rejected_without_executing_anything()
    {
        var dispatcher = new ToolDispatcher([new FakeTool("only_allowed_tool")]);

        var result = await dispatcher.DispatchAsync("delete_everything", "{}", CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.UnknownTool, result.Status);
    }

    [Fact]
    public async Task Known_tool_name_dispatches_to_the_matching_tool_only()
    {
        var targetTool = new FakeTool("target_tool");
        var otherTool = new FakeTool("other_tool");
        var dispatcher = new ToolDispatcher([otherTool, targetTool]);

        var result = await dispatcher.DispatchAsync("target_tool", "{}", CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.Success, result.Status);
        Assert.Equal(1, targetTool.CallCount);
        Assert.Equal(0, otherTool.CallCount);
    }

    private sealed class FakeTool(string name) : ITool
    {
        public int CallCount { get; private set; }

        public string Name => name;

        public Task<ToolDispatchResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(ToolDispatchResult.Success("{}"));
        }
    }
}
