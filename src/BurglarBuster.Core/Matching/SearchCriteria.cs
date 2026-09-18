namespace BurglarBuster.Core.Matching;

/// <summary>
/// Structured search input. This is what the model is allowed to construct — never
/// SQL, never free text passed through to a query (see
/// BURGLAR_BUSTER_PROJECT_SPEC.md §6, §9). All fields are optional; how much is
/// populated determines whether a search is even attempted (see
/// <see cref="OutcomePolicy.IsSufficient" />).
/// </summary>
public sealed record SearchCriteria
{
    public string? GivenName { get; init; }

    public string? MiddleNames { get; init; }

    public string? FamilyName { get; init; }

    /// <summary>Alternative to Given/Family when the operator gave one combined name.</summary>
    public string? FullName { get; init; }

    /// <summary>A name the person is believed to go by that isn't their recorded legal name.</summary>
    public string? AliasName { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    /// <summary>Used only when an exact date of birth isn't available.</summary>
    public int? ApproximateAge { get; init; }

    /// <summary>Free-text fragment of a street address (current or previous).</summary>
    public string? AddressFragment { get; init; }

    public string? Locality { get; init; }

    public string? State { get; init; }

    public string? Postcode { get; init; }

    public int? ApproximateHeightCm { get; init; }

    public string? EyeColour { get; init; }

    public string? HairColour { get; init; }

    public string? DistinguishingMark { get; init; }
}
