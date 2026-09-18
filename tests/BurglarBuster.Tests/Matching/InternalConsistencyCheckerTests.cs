using BurglarBuster.Core.Entities;
using BurglarBuster.Core.Matching;

namespace BurglarBuster.Tests.Matching;

public class InternalConsistencyCheckerTests
{
    [Fact]
    public void Large_height_discrepancy_across_observations_is_flagged()
    {
        var person = CreatePerson();
        AddDescription(person, height: 170, date: new DateOnly(2019, 1, 1));
        AddDescription(person, height: 190, date: new DateOnly(2023, 1, 1));

        var conflicts = InternalConsistencyChecker.GetInternalConflicts(person);

        Assert.Contains(conflicts, c => c.Contains("height varies"));
    }

    [Fact]
    public void Small_height_difference_is_not_flagged()
    {
        var person = CreatePerson();
        AddDescription(person, height: 178, date: new DateOnly(2019, 1, 1));
        AddDescription(person, height: 180, date: new DateOnly(2023, 1, 1));

        var conflicts = InternalConsistencyChecker.GetInternalConflicts(person);

        Assert.Empty(conflicts);
    }

    [Fact]
    public void Single_observation_is_never_flagged()
    {
        var person = CreatePerson();
        AddDescription(person, height: 300, date: new DateOnly(2019, 1, 1));

        var conflicts = InternalConsistencyChecker.GetInternalConflicts(person);

        Assert.Empty(conflicts);
    }

    private static Person CreatePerson() => new()
    {
        PersonId = "BB-TEST",
        GivenName = "A",
        FamilyName = "B",
        RecordStatus = RecordStatus.Active,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow,
    };

    private static void AddDescription(Person person, int height, DateOnly date)
    {
        person.PhysicalDescriptions.Add(new PhysicalDescription
        {
            PersonId = person.PersonId,
            ApproximateHeightCm = height,
            ObservedOn = date,
        });
    }
}
