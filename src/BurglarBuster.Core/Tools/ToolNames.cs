namespace BurglarBuster.Core.Tools;

/// <summary>
/// The complete, fixed set of tools the model may call — the allowlist itself (spec
/// §10). No generic SQL/shell/HTTP/file/code-execution tool exists or may be added
/// here without an explicit scope change (see CLAUDE.md).
/// </summary>
public static class ToolNames
{
    public const string SearchPeople = "search_people";
    public const string GetPersonRecord = "get_person_record";
    public const string GetAliases = "get_aliases";
    public const string GetAddressHistory = "get_address_history";
    public const string GetPhysicalDescription = "get_physical_description";
    public const string GetCaseReferences = "get_case_references";
    public const string CompareCandidates = "compare_candidates";

    public static readonly IReadOnlyList<string> All =
    [
        SearchPeople,
        GetPersonRecord,
        GetAliases,
        GetAddressHistory,
        GetPhysicalDescription,
        GetCaseReferences,
        CompareCandidates,
    ];
}
