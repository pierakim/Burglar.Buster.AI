using BurglarBuster.Core.Matching;

namespace BurglarBuster.Tests.Matching;

public class NameNormalizerTests
{
    [Theory]
    [InlineData("O'Brien", "OBRIEN")]
    [InlineData("  daniel  ", "DANIEL")]
    [InlineData(null, "")]
    public void Normalize_strips_punctuation_and_case(string? input, string expected)
    {
        Assert.Equal(expected, NameNormalizer.Normalize(input));
    }

    [Fact]
    public void Prefix_truncates_to_requested_length()
    {
        Assert.Equal("MILL", NameNormalizer.Prefix("Miller"));
        Assert.Equal("SOF", NameNormalizer.Prefix("Sof", 4));
    }
}
