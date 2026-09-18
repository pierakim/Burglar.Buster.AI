namespace BurglarBuster.Core.Agent;

/// <summary>The operator's raw input — always treated as untrusted data, never as
/// instructions to the model (spec §14).</summary>
public sealed record InvestigationRequest
{
    public required string Description { get; init; }
}
