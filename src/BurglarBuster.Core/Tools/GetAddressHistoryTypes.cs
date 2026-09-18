namespace BurglarBuster.Core.Tools;

public sealed record AddressHistoryEntry
{
    public required string AddressLine { get; init; }

    public required string Locality { get; init; }

    public required string State { get; init; }

    public required string Postcode { get; init; }

    public DateOnly? ValidFrom { get; init; }

    /// <summary>Null means this is the most recent address on file — not a claim the
    /// person still lives there today.</summary>
    public DateOnly? ValidTo { get; init; }
}

/// <summary>Chronological order, oldest first (spec §10).</summary>
public sealed record GetAddressHistoryResult
{
    public required IReadOnlyList<AddressHistoryEntry> Addresses { get; init; }
}
