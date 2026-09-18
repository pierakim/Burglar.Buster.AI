namespace BurglarBuster.Core.Entities;

/// <summary>
/// One ranked candidate persisted against an investigation. Evidence lists are stored
/// as JSON text columns — short string arrays, never queried into, so a value
/// converter would be overkill.
/// </summary>
public sealed class InvestigationCandidate
{
    public int Id { get; init; }

    public required string InvestigationId { get; init; }

    public required string PersonId { get; init; }

    public required int Score { get; init; }

    public required int Rank { get; init; }

    public required string MatchedEvidenceJson { get; init; }

    public required string ConflictsJson { get; init; }

    public required string MissingEvidenceJson { get; init; }

    public Investigation? Investigation { get; init; }
}
