using System.Text.Json;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Repositories;

namespace BurglarBuster.Infrastructure.Tools;

public sealed class GetAliasesTool(IPersonReadRepository repository) : ITool
{
    public string Name => ToolNames.GetAliases;

    public async Task<ToolDispatchResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        var (personId, error) = PersonIdArgumentParser.Parse(argumentsJson);
        if (error is not null)
        {
            return error;
        }

        var person = await repository.GetByIdAsync(personId!, cancellationToken);
        if (person is null)
        {
            return ToolDispatchResult.NotFound($"No record found for person ID \"{personId}\".");
        }

        var aliases = person.Aliases
            .Take(ToolPolicy.MaxAliasesReturned)
            .Select(a => new AliasSummary
            {
                GivenName = a.GivenName,
                FamilyName = a.FamilyName,
                FullName = a.FullName,
                RecordedOn = a.RecordedOn,
            })
            .ToList();

        var result = new GetAliasesResult { Aliases = aliases };

        return ToolDispatchResult.Success(JsonSerializer.Serialize(result, ToolJson.Options));
    }
}
