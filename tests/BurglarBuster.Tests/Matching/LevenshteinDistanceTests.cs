using BurglarBuster.Core.Matching;

namespace BurglarBuster.Tests.Matching;

public class LevenshteinDistanceTests
{
    [Theory]
    [InlineData("MILLER", "MILLER", 0)]
    [InlineData("MILLER", "MILLAR", 1)]
    [InlineData("SOPHIA", "SOFIA", 2)]
    [InlineData("", "ABC", 3)]
    [InlineData("KITTEN", "SITTING", 3)]
    public void Compute_matches_expected_edit_distance(string a, string b, int expected)
    {
        Assert.Equal(expected, LevenshteinDistance.Compute(a, b));
    }
}
