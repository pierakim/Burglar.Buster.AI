using System.Text.Json;
using BurglarBuster.Core.Matching;
using BurglarBuster.Core.Tools;

namespace BurglarBuster.Tests.Tools;

public class ToolCatalogTests
{
    [Fact]
    public void Catalog_defines_exactly_the_allowlisted_tools()
    {
        var catalogNames = ToolCatalog.Definitions.Select(d => d.Name).OrderBy(n => n, StringComparer.Ordinal);
        var allowlistNames = ToolNames.All.OrderBy(n => n, StringComparer.Ordinal);

        Assert.Equal(allowlistNames, catalogNames);
    }

    [Fact]
    public void Every_definition_has_a_non_empty_description()
    {
        Assert.All(ToolCatalog.Definitions, d => Assert.False(string.IsNullOrWhiteSpace(d.Description)));
    }

    [Theory]
    [MemberData(nameof(ToolDefinitions))]
    public void Every_schema_is_valid_json(ToolDefinition definition)
    {
        var exception = Record.Exception(() => JsonDocument.Parse(definition.ParametersSchemaJson));

        Assert.Null(exception);
    }

    [Theory]
    [MemberData(nameof(ToolDefinitions))]
    public void Every_schema_declares_an_object_type_with_no_additional_properties(ToolDefinition definition)
    {
        using var document = JsonDocument.Parse(definition.ParametersSchemaJson);
        var root = document.RootElement;

        Assert.Equal("object", root.GetProperty("type").GetString());
        Assert.False(root.GetProperty("additionalProperties").GetBoolean());
    }

    [Fact]
    public void Compare_candidates_schema_bound_matches_the_matching_policy_constant()
    {
        var definition = ToolCatalog.Definitions.Single(d => d.Name == ToolNames.CompareCandidates);
        using var document = JsonDocument.Parse(definition.ParametersSchemaJson);

        var maxItems = document.RootElement.GetProperty("properties").GetProperty("personIds").GetProperty("maxItems").GetInt32();

        Assert.Equal(MatchingPolicy.MaxDetailedCandidates, maxItems);
    }

    public static IEnumerable<object[]> ToolDefinitions() => ToolCatalog.Definitions.Select(d => new object[] { d });
}
