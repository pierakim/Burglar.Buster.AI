using BurglarBuster.Core.Matching;

namespace BurglarBuster.Tests.Matching;

public class OutcomePolicyTests
{
    [Fact]
    public void Empty_criteria_is_insufficient()
    {
        Assert.False(OutcomePolicy.IsSufficient(new SearchCriteria()));
    }

    [Fact]
    public void Appearance_only_criteria_is_insufficient()
    {
        var criteria = new SearchCriteria { EyeColour = "Brown", HairColour = "Black" };

        Assert.False(OutcomePolicy.IsSufficient(criteria));
    }

    [Theory]
    [MemberData(nameof(SufficientCriteria))]
    public void Criteria_with_an_identifying_field_is_sufficient(SearchCriteria criteria)
    {
        Assert.True(OutcomePolicy.IsSufficient(criteria));
    }

    public static IEnumerable<object[]> SufficientCriteria()
    {
        yield return new object[] { new SearchCriteria { FamilyName = "Miller" } };
        yield return new object[] { new SearchCriteria { DateOfBirth = new DateOnly(1990, 1, 1) } };
        yield return new object[] { new SearchCriteria { ApproximateAge = 30 } };
        yield return new object[] { new SearchCriteria { Locality = "Silverbrook" } };
    }
}
