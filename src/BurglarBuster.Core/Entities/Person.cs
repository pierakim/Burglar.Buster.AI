namespace BurglarBuster.Core.Entities;

/// <summary>
/// A synthetic person record. Read-only after seeding — see CLAUDE.md: the agent
/// must never create, modify, merge or delete a person record.
/// </summary>
public sealed class Person
{
    /// <summary>Stable fictional identifier, e.g. "BB-1042".</summary>
    public required string PersonId { get; init; }

    public required string GivenName { get; init; }

    public string? MiddleNames { get; init; }

    public required string FamilyName { get; init; }

    /// <summary>Optional — some records deliberately have an incomplete or unknown DOB.</summary>
    public DateOnly? DateOfBirth { get; init; }

    public required RecordStatus RecordStatus { get; init; }

    public required DateTimeOffset CreatedAtUtc { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }

    public ICollection<PersonAlias> Aliases { get; init; } = new List<PersonAlias>();

    public ICollection<PersonAddress> Addresses { get; init; } = new List<PersonAddress>();

    public ICollection<PhysicalDescription> PhysicalDescriptions { get; init; } = new List<PhysicalDescription>();

    public ICollection<CaseReference> CaseReferences { get; init; } = new List<CaseReference>();
}
