namespace BurglarBuster.Core.Tools;

public sealed record AliasSummary
{
    public string? GivenName { get; init; }

    public string? FamilyName { get; init; }

    public required string FullName { get; init; }

    public DateOnly? RecordedOn { get; init; }
}

public sealed record GetAliasesResult
{
    public required IReadOnlyList<AliasSummary> Aliases { get; init; }
}
