namespace BurglarBuster.Core.Entities;

/// <summary>
/// A single synthetic physical observation. A person may have more than one — recorded
/// on different dates, sometimes disagreeing with each other (see the conflicting-
/// information fixture scenario). Weak supporting evidence only, never a strong match
/// on its own — see CLAUDE.md.
/// </summary>
public sealed class PhysicalDescription
{
    public int Id { get; init; }

    public required string PersonId { get; init; }

    public int? ApproximateHeightCm { get; init; }

    public string? EyeColour { get; init; }

    public string? HairColour { get; init; }

    public string? DistinguishingMarks { get; init; }

    public required DateOnly ObservedOn { get; init; }

    public Person? Person { get; init; }
}
