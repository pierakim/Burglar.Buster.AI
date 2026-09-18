namespace BurglarBuster.Core.Entities;

/// <summary>
/// A known alias for a person — a full nickname, or a given/family name variant.
/// Not all fields are always populated (e.g. a single-word nickname has no family name).
/// </summary>
public sealed class PersonAlias
{
    public int Id { get; init; }

    public required string PersonId { get; init; }

    public string? GivenName { get; init; }

    public string? FamilyName { get; init; }

    /// <summary>Always populated — the display form of the alias, however it was recorded.</summary>
    public required string FullName { get; init; }

    public DateOnly? RecordedOn { get; init; }

    public Person? Person { get; init; }
}
