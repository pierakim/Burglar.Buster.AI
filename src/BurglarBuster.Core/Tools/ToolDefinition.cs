namespace BurglarBuster.Core.Tools;

/// <summary>What Milestone 5 will hand the Foundry model for one tool.</summary>
public sealed record ToolDefinition
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    /// <summary>JSON Schema describing the arguments object.</summary>
    public required string ParametersSchemaJson { get; init; }
}
