using System.Text.Json;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Repositories;

namespace BurglarBuster.Infrastructure.Tools;

public sealed class GetAddressHistoryTool(IPersonReadRepository repository) : ITool
{
    public string Name => ToolNames.GetAddressHistory;

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

        var addresses = person.Addresses
            .OrderBy(a => a.ValidFrom ?? DateOnly.MinValue)
            .ThenBy(a => a.Id)
            .Take(ToolPolicy.MaxAddressesReturned)
            .Select(a => new AddressHistoryEntry
            {
                AddressLine = a.AddressLine,
                Locality = a.Locality,
                State = a.State,
                Postcode = a.Postcode,
                ValidFrom = a.ValidFrom,
                ValidTo = a.ValidTo,
            })
            .ToList();

        var result = new GetAddressHistoryResult { Addresses = addresses };

        return ToolDispatchResult.Success(JsonSerializer.Serialize(result, ToolJson.Options));
    }
}
