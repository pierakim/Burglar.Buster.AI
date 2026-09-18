namespace BurglarBuster.Core.Agent;

/// <summary>
/// The JSON shape the model's final (non-tool-call) reply must match. Deliberately
/// has NO outcome/score/confidence field — the model never states or computes the
/// investigation outcome; that's always derived by the host from the deterministic
/// evidence gathered during the loop (spec §6, §14).
/// </summary>
public sealed record ProposedFinalResponse
{
    public required string Summary { get; init; }

    public IReadOnlyList<string>? RequestedInformation { get; init; }
}
