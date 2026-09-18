using System.Text.Json;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Repositories;

namespace BurglarBuster.Infrastructure.Tools;

public sealed class GetPersonRecordTool(IPersonReadRepository repository) : ITool
{
    public string Name => ToolNames.GetPersonRecord;

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

        var result = new PersonRecordResult
        {
            PersonId = person.PersonId,
            GivenName = person.GivenName,
            MiddleNames = person.MiddleNames,
            FamilyName = person.FamilyName,
            DateOfBirth = person.DateOfBirth,
            RecordStatus = person.RecordStatus.ToString(),
        };

        return ToolDispatchResult.Success(JsonSerializer.Serialize(result, ToolJson.Options));
    }
}
