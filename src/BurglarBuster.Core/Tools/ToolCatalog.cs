namespace BurglarBuster.Core.Tools;

/// <summary>
/// Static, hand-authored JSON Schemas for every allowed tool — Milestone 4 defines
/// these; Milestone 5 is what actually sends them to the Foundry model. Kept in sync
/// with the C# argument DTOs in this folder by
/// <c>ToolCatalogTests</c>/<c>ToolSchemaValidationTests</c>, not by code generation —
/// there are only seven of these and hand-writing keeps the JSON Schema exactly as
/// simple as the model needs, per spec §10.
/// </summary>
public static class ToolCatalog
{
    private const string SearchCriteriaPropertiesJson = """
        "givenName": { "type": "string" },
        "middleNames": { "type": "string" },
        "familyName": { "type": "string" },
        "fullName": { "type": "string", "description": "Use when only a single combined name is known." },
        "aliasName": { "type": "string", "description": "A name the person is believed to go by that may not be their legal name." },
        "dateOfBirth": { "type": "string", "format": "date", "description": "ISO 8601 date, e.g. 1989-04-12. Use only when exact." },
        "approximateAge": { "type": "integer", "description": "Use only when an exact date of birth isn't available." },
        "addressFragment": { "type": "string", "description": "A fragment of a street address, current or previous." },
        "locality": { "type": "string" },
        "state": { "type": "string" },
        "postcode": { "type": "string" },
        "approximateHeightCm": { "type": "integer" },
        "eyeColour": { "type": "string" },
        "hairColour": { "type": "string" },
        "distinguishingMark": { "type": "string" }
        """;

    public static readonly IReadOnlyList<ToolDefinition> Definitions =
    [
        new ToolDefinition
        {
            Name = ToolNames.SearchPeople,
            Description = "Searches synthetic person records using structured criteria extracted from the operator's description. Returns a bounded, ranked list of candidates with field-level evidence. Every field is optional, but at least a name, date of birth/approximate age, or address hint is needed for a reliable search.",
            ParametersSchemaJson = $$"""
                {
                  "type": "object",
                  "properties": {
                    {{SearchCriteriaPropertiesJson}}
                  },
                  "additionalProperties": false
                }
                """,
        },
        new ToolDefinition
        {
            Name = ToolNames.GetPersonRecord,
            Description = "Returns the core record (name, date of birth, record status) for one person ID already returned by search_people. Does not include aliases, addresses, physical descriptions or case references — use the dedicated tools for those.",
            ParametersSchemaJson = PersonIdOnlySchema,
        },
        new ToolDefinition
        {
            Name = ToolNames.GetAliases,
            Description = "Returns known aliases for one person ID.",
            ParametersSchemaJson = PersonIdOnlySchema,
        },
        new ToolDefinition
        {
            Name = ToolNames.GetAddressHistory,
            Description = "Returns current and previous addresses for one person ID, oldest first.",
            ParametersSchemaJson = PersonIdOnlySchema,
        },
        new ToolDefinition
        {
            Name = ToolNames.GetPhysicalDescription,
            Description = "Returns known physical observations (height, eye colour, hair colour, distinguishing marks) for one person ID. This is weak supporting evidence only and can never establish a strong match by itself.",
            ParametersSchemaJson = PersonIdOnlySchema,
        },
        new ToolDefinition
        {
            Name = ToolNames.GetCaseReferences,
            Description = "Returns a bounded list of fictional case references for one person ID, for context only. Case references never affect identity similarity and must never be used to infer guilt, risk, or any legal conclusion.",
            ParametersSchemaJson = PersonIdOnlySchema,
        },
        new ToolDefinition
        {
            Name = ToolNames.CompareCandidates,
            Description = "Runs a deterministic, field-by-field comparison of up to 3 person IDs against the original search criteria. Returns agreements, differences, missing data and rank for each — never a model-computed score.",
            ParametersSchemaJson = $$"""
                {
                  "type": "object",
                  "properties": {
                    "criteria": {
                      "type": "object",
                      "properties": {
                        {{SearchCriteriaPropertiesJson}}
                      },
                      "additionalProperties": false
                    },
                    "personIds": {
                      "type": "array",
                      "items": { "type": "string" },
                      "minItems": 1,
                      "maxItems": 3,
                      "description": "Person IDs to compare in detail, at most 3."
                    }
                  },
                  "required": ["criteria", "personIds"],
                  "additionalProperties": false
                }
                """,
        },
    ];

    private const string PersonIdOnlySchema = """
        {
          "type": "object",
          "properties": {
            "personId": { "type": "string", "description": "Stable person identifier returned by search_people, e.g. BB-1042." }
          },
          "required": ["personId"],
          "additionalProperties": false
        }
        """;
}
