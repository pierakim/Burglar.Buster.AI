namespace BurglarBuster.Core.Tools;

/// <summary>
/// Shared argument shape for every tool that takes just a person ID
/// (get_person_record, get_aliases, get_address_history, get_physical_description,
/// get_case_references).
/// </summary>
public sealed record PersonIdArguments
{
    public required string PersonId { get; init; }
}
