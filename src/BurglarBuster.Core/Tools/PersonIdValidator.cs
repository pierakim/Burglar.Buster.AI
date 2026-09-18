using System.Text.RegularExpressions;

namespace BurglarBuster.Core.Tools;

/// <summary>
/// Validates the stable fictional identifier format ("BB-1042") before any tool ever
/// touches the database with it — malformed IDs are rejected as invalid arguments,
/// never as a database lookup that happens to find nothing (spec §17's "malformed
/// model tool arguments" vs. "invalid person ID" are two different failure modes).
/// </summary>
public static partial class PersonIdValidator
{
    public static bool IsValidFormat(string? personId) =>
        !string.IsNullOrEmpty(personId) && PersonIdPattern().IsMatch(personId);

    [GeneratedRegex(@"^BB-\d{4,}$")]
    private static partial Regex PersonIdPattern();
}
