namespace BurglarBuster.Core.Tools;

/// <summary>The core Person record only — aliases/addresses/physical descriptions/case
/// references are separate tools, each with their own bounds.</summary>
public sealed record PersonRecordResult
{
    public required string PersonId { get; init; }

    public required string GivenName { get; init; }

    public string? MiddleNames { get; init; }

    public required string FamilyName { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    public required string RecordStatus { get; init; }
}
