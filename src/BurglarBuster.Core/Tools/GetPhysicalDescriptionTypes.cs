namespace BurglarBuster.Core.Tools;

public sealed record PhysicalDescriptionObservation
{
    public int? ApproximateHeightCm { get; init; }

    public string? EyeColour { get; init; }

    public string? HairColour { get; init; }

    public string? DistinguishingMarks { get; init; }

    public required DateOnly ObservedOn { get; init; }
}

public sealed record GetPhysicalDescriptionResult
{
    public required IReadOnlyList<PhysicalDescriptionObservation> Observations { get; init; }
}
