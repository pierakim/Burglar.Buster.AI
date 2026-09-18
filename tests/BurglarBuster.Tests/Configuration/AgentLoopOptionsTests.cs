using System.ComponentModel.DataAnnotations;
using BurglarBuster.Core.Agent;

namespace BurglarBuster.Tests.Configuration;

public class AgentLoopOptionsTests
{
    [Fact]
    public void Defaults_match_spec_values_and_are_valid()
    {
        var options = new AgentLoopOptions();

        Assert.Equal(6, options.MaxTurns);
        Assert.Equal(8, options.MaxToolCalls);
        Assert.Equal(3, options.MaxDetailedCandidates);
        Assert.Equal(30, options.UpstreamTimeoutSeconds);
        Assert.Equal(60, options.OverallTimeoutSeconds);
        Assert.True(TryValidate(options, out _));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MaxTurns_below_one_is_invalid(int invalidValue)
    {
        var options = new AgentLoopOptions { MaxTurns = invalidValue };

        Assert.False(TryValidate(options, out var results));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AgentLoopOptions.MaxTurns)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MaxToolCalls_below_one_is_invalid(int invalidValue)
    {
        var options = new AgentLoopOptions { MaxToolCalls = invalidValue };

        Assert.False(TryValidate(options, out var results));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AgentLoopOptions.MaxToolCalls)));
    }

    private static bool TryValidate(AgentLoopOptions options, out List<ValidationResult> results)
    {
        var context = new ValidationContext(options);
        results = new List<ValidationResult>();
        return Validator.TryValidateObject(options, context, results, validateAllProperties: true);
    }
}
