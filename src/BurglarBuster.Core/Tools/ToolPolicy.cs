namespace BurglarBuster.Core.Tools;

/// <summary>
/// Result bounds for the non-search tools. search_people/compare_candidates reuse
/// <see cref="Matching.MatchingPolicy.MaxCandidatesReturned" />/
/// <see cref="Matching.MatchingPolicy.MaxDetailedCandidates" /> directly rather than
/// duplicating those numbers here.
/// </summary>
public static class ToolPolicy
{
    public const int MaxAliasesReturned = 20;
    public const int MaxAddressesReturned = 20;
    public const int MaxPhysicalDescriptionsReturned = 20;
    public const int MaxCaseReferencesReturned = 10;
}
