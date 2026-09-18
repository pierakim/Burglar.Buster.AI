namespace BurglarBuster.Core.Tools;

/// <summary>Context only — never used for identity similarity scoring (spec §7, §9).</summary>
public sealed record CaseReferenceSummary
{
    public required string ReferenceNumber { get; init; }

    public required string Summary { get; init; }

    public required DateOnly Date { get; init; }

    public required string Status { get; init; }
}

public sealed record GetCaseReferencesResult
{
    public required IReadOnlyList<CaseReferenceSummary> CaseReferences { get; init; }
}
