using BurglarBuster.Core.Entities;

namespace BurglarBuster.Infrastructure.Seeding;

/// <summary>
/// Generates plain filler people to reach the ~1,000-record target — deterministic
/// given the same seed, but otherwise unremarkable (one current address, one physical
/// description, no aliases/case references/previous addresses). The interesting
/// relational scenarios all live in <see cref="FixtureScenarios"/> instead, so this
/// generator can stay simple.
/// </summary>
internal static class BulkPersonGenerator
{
    private static readonly DateTimeOffset SeedEpoch = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<Person> Generate(int startNumber, int count, int randomSeed)
    {
        var random = new Random(randomSeed);
        var people = new List<Person>(count);

        for (var i = 0; i < count; i++)
        {
            var personId = PersonIdFormatter.Format(startNumber + i);
            var createdAt = SeedEpoch.AddMinutes(i);

            var (locality, state) = SyntheticDataCatalog.Localities[random.Next(SyntheticDataCatalog.Localities.Length)];
            var address = new PersonAddress
            {
                PersonId = personId,
                AddressLine = $"{random.Next(1, 400)} {Pick(random, SyntheticDataCatalog.StreetNames)} {Pick(random, SyntheticDataCatalog.StreetTypes)}",
                Locality = locality,
                State = state,
                Postcode = random.Next(1000, 9999).ToString(),
                ValidFrom = DateOnly.FromDateTime(createdAt.UtcDateTime).AddYears(-random.Next(1, 15)),
                ValidTo = null,
            };

            var description = new PhysicalDescription
            {
                PersonId = personId,
                ApproximateHeightCm = random.Next(155, 196),
                EyeColour = Pick(random, SyntheticDataCatalog.EyeColours),
                HairColour = Pick(random, SyntheticDataCatalog.HairColours),
                DistinguishingMarks = null,
                ObservedOn = DateOnly.FromDateTime(createdAt.UtcDateTime),
            };

            var dateOfBirth = DateOnly.FromDateTime(createdAt.UtcDateTime)
                .AddYears(-random.Next(18, 86))
                .AddDays(-random.Next(0, 365));

            people.Add(new Person
            {
                PersonId = personId,
                GivenName = Pick(random, SyntheticDataCatalog.GivenNames),
                FamilyName = Pick(random, SyntheticDataCatalog.FamilyNames),
                DateOfBirth = dateOfBirth,
                RecordStatus = random.Next(100) < 5 ? RecordStatus.Archived : RecordStatus.Active,
                CreatedAtUtc = createdAt,
                UpdatedAtUtc = createdAt,
                Addresses = [address],
                PhysicalDescriptions = [description],
            });
        }

        return people;
    }

    private static string Pick(Random random, string[] pool) => pool[random.Next(pool.Length)];
}
