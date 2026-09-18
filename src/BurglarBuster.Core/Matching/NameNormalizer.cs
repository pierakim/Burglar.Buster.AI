namespace BurglarBuster.Core.Matching;

/// <summary>
/// Normalizes free-text names for comparison: trims, upper-invariants, and drops
/// punctuation (apostrophes, hyphens) so "O'Brien" and "OBrien" compare equal.
/// </summary>
internal static class NameNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var chars = value.Trim().ToUpperInvariant()
            .Where(c => char.IsLetterOrDigit(c) || c == ' ')
            .ToArray();

        return new string(chars);
    }

    /// <summary>
    /// A short, stable prefix (up to 4 normalized characters) suitable for a broad
    /// recall-oriented database LIKE pattern — long enough to filter usefully, short
    /// enough to still catch common misspellings (e.g. "Miller" / "Millar" share "MILL").
    /// </summary>
    public static string Prefix(string? value, int length = 4)
    {
        var normalized = Normalize(value);
        return normalized.Length <= length ? normalized : normalized[..length];
    }
}
