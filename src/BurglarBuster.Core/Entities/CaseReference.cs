namespace BurglarBuster.Core.Entities;

/// <summary>
/// A fictional case reference giving context about a record. Contextual only — its
/// presence must never be used as identity-match score or proof of guilt (spec §7,
/// enforced by the matching engine in Milestone 3 never reading this table for scoring).
/// </summary>
public sealed class CaseReference
{
    public int Id { get; init; }

    public required string PersonId { get; init; }

    /// <summary>Fictional reference number, e.g. "CR-2024-00118".</summary>
    public required string ReferenceNumber { get; init; }

    public required string Summary { get; init; }

    public required DateOnly Date { get; init; }

    public required string Status { get; init; }

    public Person? Person { get; init; }
}
