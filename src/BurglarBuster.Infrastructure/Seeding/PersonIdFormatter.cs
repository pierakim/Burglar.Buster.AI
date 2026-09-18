namespace BurglarBuster.Infrastructure.Seeding;

internal static class PersonIdFormatter
{
    /// <summary>Formats a stable fictional identifier, e.g. 1042 -&gt; "BB-1042".</summary>
    public static string Format(int number) => $"BB-{number:D4}";
}
