using BurglarBuster.Core.Agent;
using BurglarBuster.Core.Tools;

namespace BurglarBuster.Tests.TestSupport;

/// <summary>
/// A scripted fake for <see cref="IChatCompletionClient" /> — normal tests never call a
/// real model (spec §18). Returns each entry in <paramref name="responses"/> in order;
/// throws if more calls happen than were scripted. Records every call's messages/tools
/// for assertions (e.g. confirming the raw description reached the conversation
/// unmodified, or that the previous turn's tool results were appended correctly).
/// </summary>
public sealed class FakeChatCompletionClient(params IReadOnlyList<ChatCompletionResult> responses) : IChatCompletionClient
{
    private int _callIndex;

    public List<(IReadOnlyList<ChatMessage> Messages, IReadOnlyList<ToolDefinition> Tools)> Calls { get; } = [];

    public Func<int, IReadOnlyList<ChatMessage>, Exception?>? ThrowOnCall { get; set; }

    /// <summary>When set, awaited (respecting cancellation) before returning each response — for timeout tests.</summary>
    public TimeSpan? DelayPerCall { get; set; }

    public async Task<ChatCompletionResult> CompleteAsync(IReadOnlyList<ChatMessage> messages, IReadOnlyList<ToolDefinition> tools, CancellationToken cancellationToken)
    {
        Calls.Add((messages, tools));

        var exception = ThrowOnCall?.Invoke(_callIndex, messages);
        if (exception is not null)
        {
            throw exception;
        }

        if (DelayPerCall is { } delay)
        {
            await Task.Delay(delay, cancellationToken);
        }

        if (_callIndex >= responses.Count)
        {
            throw new InvalidOperationException($"FakeChatCompletionClient received more calls ({_callIndex + 1}) than were scripted ({responses.Count}).");
        }

        return responses[_callIndex++];
    }
}
