using BurglarBuster.Core.Tools;

namespace BurglarBuster.Tests.Tools;

public class PersonIdValidatorTests
{
    [Theory]
    [InlineData("BB-0001")]
    [InlineData("BB-1042")]
    [InlineData("BB-99999")]
    public void Valid_formats_are_accepted(string personId)
    {
        Assert.True(PersonIdValidator.IsValidFormat(personId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("BB-1")]
    [InlineData("BB1042")]
    [InlineData("bb-1042")]
    [InlineData("AA-1042")]
    [InlineData("BB-104A")]
    [InlineData(" BB-1042")]
    public void Invalid_formats_are_rejected(string? personId)
    {
        Assert.False(PersonIdValidator.IsValidFormat(personId));
    }
}
