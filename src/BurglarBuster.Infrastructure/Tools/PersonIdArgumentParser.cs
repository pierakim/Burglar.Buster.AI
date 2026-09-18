using System.Text.Json;
using BurglarBuster.Core.Tools;

namespace BurglarBuster.Infrastructure.Tools;

/// <summary>Shared parsing/validation for the five tools that take just a person ID.</summary>
internal static class PersonIdArgumentParser
{
    public static (string? PersonId, ToolDispatchResult? Error) Parse(string argumentsJson)
    {
        PersonIdArguments? args;
        try
        {
            args = JsonSerializer.Deserialize<PersonIdArguments>(argumentsJson, ToolJson.Options);
        }
        catch (JsonException)
        {
            return (null, ToolDispatchResult.InvalidArguments("Arguments must be a JSON object with a \"personId\" string field."));
        }

        if (args is null || !PersonIdValidator.IsValidFormat(args.PersonId))
        {
            return (null, ToolDispatchResult.InvalidArguments("\"personId\" is required and must match the format \"BB-####\"."));
        }

        return (args.PersonId, null);
    }
}
