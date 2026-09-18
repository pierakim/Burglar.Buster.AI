using System.Text.Json;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Repositories;

namespace BurglarBuster.Infrastructure.Tools;

public sealed class GetPhysicalDescriptionTool(IPersonReadRepository repository) : ITool
{
    public string Name => ToolNames.GetPhysicalDescription;

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

        var observations = person.PhysicalDescriptions
            .OrderBy(d => d.ObservedOn)
            .Take(ToolPolicy.MaxPhysicalDescriptionsReturned)
            .Select(d => new PhysicalDescriptionObservation
            {
                ApproximateHeightCm = d.ApproximateHeightCm,
                EyeColour = d.EyeColour,
                HairColour = d.HairColour,
                DistinguishingMarks = d.DistinguishingMarks,
                ObservedOn = d.ObservedOn,
            })
            .ToList();

        var result = new GetPhysicalDescriptionResult { Observations = observations };

        return ToolDispatchResult.Success(JsonSerializer.Serialize(result, ToolJson.Options));
    }
}
