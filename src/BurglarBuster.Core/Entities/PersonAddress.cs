namespace BurglarBuster.Core.Entities;

/// <summary>
/// A current or previous address. <see cref="ValidTo"/> null means "current as far as
/// the record shows" — it is not an assertion the person still lives there today.
/// </summary>
public sealed class PersonAddress
{
    public int Id { get; init; }

    public required string PersonId { get; init; }

    public required string AddressLine { get; init; }

    public required string Locality { get; init; }

    public required string State { get; init; }

    public required string Postcode { get; init; }

    public DateOnly? ValidFrom { get; init; }

    public DateOnly? ValidTo { get; init; }

    public Person? Person { get; init; }
}
