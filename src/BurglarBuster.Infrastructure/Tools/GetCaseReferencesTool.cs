using System.Text.Json;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Repositories;

namespace BurglarBuster.Infrastructure.Tools;

public sealed class GetCaseReferencesTool(IPersonReadRepository repository) : ITool
{
    public string Name => ToolNames.GetCaseReferences;

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

        var caseReferences = person.CaseReferences
            .OrderByDescending(c => c.Date)
            .Take(ToolPolicy.MaxCaseReferencesReturned)
            .Select(c => new CaseReferenceSummary
            {
                ReferenceNumber = c.ReferenceNumber,
                Summary = c.Summary,
                Date = c.Date,
                Status = c.Status,
            })
            .ToList();

        var result = new GetCaseReferencesResult { CaseReferences = caseReferences };

        return ToolDispatchResult.Success(JsonSerializer.Serialize(result, ToolJson.Options));
    }
}
