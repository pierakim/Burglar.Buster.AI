using BurglarBuster.Core.Entities;

namespace BurglarBuster.Infrastructure.Seeding;

/// <summary>
/// Hand-authored, stable fixture records covering the controlled scenarios required by
/// BURGLAR_BUSTER_PROJECT_SPEC.md §8 (unique match, shared surnames, misspelled names,
/// aliases, previous addresses, incomplete DOB, similar appearance, ambiguous
/// candidates, conflicting evidence, potential duplicates, incomplete records). IDs
/// BB-0001..BB-0015 are reserved for these fixtures and are never touched by the
/// random bulk generator. See docs/fixture-scenarios.md for the human-readable index —
/// these IDs are intentionally not surfaced to the model's prompt.
/// </summary>
internal static class FixtureScenarios
{
    private static readonly DateTimeOffset SeedEpoch = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<Person> GetAll()
    {
        var index = 0;
        DateTimeOffset NextCreatedAt() => SeedEpoch.AddMinutes(index++);

        var people = new List<Person>
        {
            // BB-0001 — unique exact match (the spec's own §13 HTTP example fixture).
            // Previous address in Camden Reach, matching "previously lived near Camden".
            new()
            {
                PersonId = "BB-0001",
                GivenName = "Daniel",
                FamilyName = "Miller",
                DateOfBirth = new DateOnly(1989, 4, 12),
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
                Addresses =
                [
                    new PersonAddress { PersonId = "BB-0001", AddressLine = "12 Birchwood Street", Locality = "Camden Reach", State = "SB", Postcode = "4820", ValidFrom = new DateOnly(2015, 3, 1), ValidTo = new DateOnly(2020, 6, 30) },
                    new PersonAddress { PersonId = "BB-0001", AddressLine = "45 Riverside Lane", Locality = "Silverbrook", State = "AV", Postcode = "4712", ValidFrom = new DateOnly(2020, 7, 1), ValidTo = null },
                ],
                PhysicalDescriptions =
                [
                    new PhysicalDescription { PersonId = "BB-0001", ApproximateHeightCm = 180, EyeColour = "Brown", HairColour = "Brown", ObservedOn = new DateOnly(2023, 5, 10) },
                ],
                CaseReferences =
                [
                    new CaseReference { PersonId = "BB-0001", ReferenceNumber = "CR-2021-00118", Summary = "Routine identity verification, no further action.", Date = new DateOnly(2021, 8, 2), Status = "Closed" },
                ],
            },

            // BB-0002, BB-0003 — share the Miller surname with BB-0001 but are unrelated people.
            new()
            {
                PersonId = "BB-0002",
                GivenName = "Robert",
                FamilyName = "Miller",
                DateOfBirth = new DateOnly(1965, 11, 2),
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
                Addresses = [new PersonAddress { PersonId = "BB-0002", AddressLine = "8 Orchard Road", Locality = "Fernhollow", State = "FH", Postcode = "3311", ValidFrom = new DateOnly(2010, 1, 1), ValidTo = null }],
                PhysicalDescriptions = [new PhysicalDescription { PersonId = "BB-0002", ApproximateHeightCm = 175, EyeColour = "Blue", HairColour = "Grey", ObservedOn = new DateOnly(2022, 2, 14) }],
            },
            new()
            {
                PersonId = "BB-0003",
                GivenName = "Emily",
                FamilyName = "Miller",
                DateOfBirth = new DateOnly(1998, 7, 19),
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
                Addresses = [new PersonAddress { PersonId = "BB-0003", AddressLine = "3 Kestrel Court", Locality = "Millbrook", State = "FH", Postcode = "3298", ValidFrom = new DateOnly(2019, 5, 1), ValidTo = null }],
                PhysicalDescriptions = [new PhysicalDescription { PersonId = "BB-0003", ApproximateHeightCm = 165, EyeColour = "Green", HairColour = "Black", ObservedOn = new DateOnly(2023, 1, 20) }],
            },

            // BB-0004 — similar/misspelled name variant of BB-0001 ("Millar" vs "Miller"), different person.
            new()
            {
                PersonId = "BB-0004",
                GivenName = "Daniel",
                FamilyName = "Millar",
                DateOfBirth = new DateOnly(1977, 2, 28),
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
                Addresses = [new PersonAddress { PersonId = "BB-0004", AddressLine = "21 Thornbury Avenue", Locality = "Windermere", State = "AV", Postcode = "4655", ValidFrom = new DateOnly(2016, 9, 1), ValidTo = null }],
                PhysicalDescriptions = [new PhysicalDescription { PersonId = "BB-0004", ApproximateHeightCm = 172, EyeColour = "Hazel", HairColour = "Brown", ObservedOn = new DateOnly(2021, 11, 3) }],
            },

            // BB-0005 — has a recorded alias.
            new()
            {
                PersonId = "BB-0005",
                GivenName = "Robert",
                FamilyName = "Nguyen",
                DateOfBirth = new DateOnly(1992, 6, 15),
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
                Addresses = [new PersonAddress { PersonId = "BB-0005", AddressLine = "60 Union Street", Locality = "Ravenswood", State = "VC", Postcode = "2044", ValidFrom = new DateOnly(2018, 4, 1), ValidTo = null }],
                Aliases = [new PersonAlias { PersonId = "BB-0005", GivenName = "Bobby", FamilyName = "Nguyen", FullName = "Bobby Nguyen", RecordedOn = new DateOnly(2020, 3, 12) }],
                PhysicalDescriptions = [new PhysicalDescription { PersonId = "BB-0005", ApproximateHeightCm = 178, EyeColour = "Brown", HairColour = "Black", ObservedOn = new DateOnly(2022, 9, 8) }],
            },

            // BB-0006 — previous address distinct from current address.
            new()
            {
                PersonId = "BB-0006",
                GivenName = "Sarah",
                FamilyName = "Thompson",
                DateOfBirth = new DateOnly(1985, 9, 30),
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
                Addresses =
                [
                    new PersonAddress { PersonId = "BB-0006", AddressLine = "5 Larkspur Lane", Locality = "Elmsworth", State = "HR", Postcode = "3450", ValidFrom = new DateOnly(2012, 1, 1), ValidTo = new DateOnly(2019, 12, 31) },
                    new PersonAddress { PersonId = "BB-0006", AddressLine = "77 Meadowview Drive", Locality = "Stonebridge", State = "HR", Postcode = "3477", ValidFrom = new DateOnly(2020, 1, 1), ValidTo = null },
                ],
                PhysicalDescriptions = [new PhysicalDescription { PersonId = "BB-0006", ApproximateHeightCm = 168, EyeColour = "Grey", HairColour = "Blond", ObservedOn = new DateOnly(2023, 4, 17) }],
            },

            // BB-0007 — incomplete date of birth (unknown).
            new()
            {
                PersonId = "BB-0007",
                GivenName = "Michael",
                FamilyName = "Brooks",
                DateOfBirth = null,
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
                Addresses = [new PersonAddress { PersonId = "BB-0007", AddressLine = "14 Northgate Road", Locality = "Amber Valley", State = "SB", Postcode = "4801", ValidFrom = new DateOnly(2017, 6, 1), ValidTo = null }],
                PhysicalDescriptions = [new PhysicalDescription { PersonId = "BB-0007", ApproximateHeightCm = 183, EyeColour = "Blue", HairColour = "Brown", ObservedOn = new DateOnly(2022, 12, 1) }],
            },

            // BB-0008, BB-0009 — near-identical physical descriptions, different identities.
            new()
            {
                PersonId = "BB-0008",
                GivenName = "Olivia",
                FamilyName = "Carter",
                DateOfBirth = new DateOnly(1994, 3, 5),
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
                Addresses = [new PersonAddress { PersonId = "BB-0008", AddressLine = "9 Pinehurst Court", Locality = "Cedar Falls", State = "SB", Postcode = "4833", ValidFrom = new DateOnly(2019, 2, 1), ValidTo = null }],
                PhysicalDescriptions = [new PhysicalDescription { PersonId = "BB-0008", ApproximateHeightCm = 170, EyeColour = "Green", HairColour = "Red", ObservedOn = new DateOnly(2023, 6, 22) }],
            },
            new()
            {
                PersonId = "BB-0009",
                GivenName = "Chloe",
                FamilyName = "Carter",
                DateOfBirth = new DateOnly(2001, 10, 9),
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
                Addresses = [new PersonAddress { PersonId = "BB-0009", AddressLine = "31 Quarry Drive", Locality = "Thistledown", State = "AV", Postcode = "4690", ValidFrom = new DateOnly(2021, 8, 1), ValidTo = null }],
                PhysicalDescriptions = [new PhysicalDescription { PersonId = "BB-0009", ApproximateHeightCm = 170, EyeColour = "Green", HairColour = "Red", ObservedOn = new DateOnly(2023, 7, 2) }],
            },

            // BB-0010, BB-0011 — intentionally ambiguous: similar given name, same
            // surname, same approximate age, same locality.
            new()
            {
                PersonId = "BB-0010",
                GivenName = "James",
                FamilyName = "Ellison",
                DateOfBirth = new DateOnly(1984, 5, 1),
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
                Addresses = [new PersonAddress { PersonId = "BB-0010", AddressLine = "16 Vineyard Street", Locality = "Stonebridge", State = "HR", Postcode = "3477", ValidFrom = new DateOnly(2015, 1, 1), ValidTo = null }],
                PhysicalDescriptions = [new PhysicalDescription { PersonId = "BB-0010", ApproximateHeightCm = 176, EyeColour = "Brown", HairColour = "Black", ObservedOn = new DateOnly(2022, 3, 30) }],
            },
            new()
            {
                PersonId = "BB-0011",
                GivenName = "Jason",
                FamilyName = "Ellison",
                DateOfBirth = new DateOnly(1984, 8, 22),
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
                Addresses = [new PersonAddress { PersonId = "BB-0011", AddressLine = "18 Westfield Avenue", Locality = "Stonebridge", State = "HR", Postcode = "3477", ValidFrom = new DateOnly(2016, 1, 1), ValidTo = null }],
                PhysicalDescriptions = [new PhysicalDescription { PersonId = "BB-0011", ApproximateHeightCm = 177, EyeColour = "Brown", HairColour = "Black", ObservedOn = new DateOnly(2022, 4, 2) }],
            },

            // BB-0012 — conflicting evidence: two physical observations with a large,
            // unexplained height discrepancy.
            new()
            {
                PersonId = "BB-0012",
                GivenName = "William",
                FamilyName = "Foster",
                DateOfBirth = new DateOnly(1990, 12, 25),
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
                Addresses = [new PersonAddress { PersonId = "BB-0012", AddressLine = "27 Ashgrove Lane", Locality = "Maple Junction", State = "HR", Postcode = "3402", ValidFrom = new DateOnly(2014, 1, 1), ValidTo = null }],
                PhysicalDescriptions =
                [
                    new PhysicalDescription { PersonId = "BB-0012", ApproximateHeightCm = 170, EyeColour = "Blue", HairColour = "Brown", ObservedOn = new DateOnly(2019, 5, 1) },
                    new PhysicalDescription { PersonId = "BB-0012", ApproximateHeightCm = 190, EyeColour = "Blue", HairColour = "Brown", ObservedOn = new DateOnly(2023, 8, 15) },
                ],
            },

            // BB-0013, BB-0014 — potential duplicate pair: near-identical spelling, same
            // DOB, same address.
            new()
            {
                PersonId = "BB-0013",
                GivenName = "Sophia",
                FamilyName = "Grant",
                DateOfBirth = new DateOnly(1996, 1, 17),
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
                Addresses = [new PersonAddress { PersonId = "BB-0013", AddressLine = "50 Hollyfield Road", Locality = "Bridgewater Hollow", State = "VC", Postcode = "2210", ValidFrom = new DateOnly(2020, 3, 1), ValidTo = null }],
                PhysicalDescriptions = [new PhysicalDescription { PersonId = "BB-0013", ApproximateHeightCm = 163, EyeColour = "Hazel", HairColour = "Brown", ObservedOn = new DateOnly(2023, 2, 11) }],
            },
            new()
            {
                PersonId = "BB-0014",
                GivenName = "Sofia",
                FamilyName = "Grant",
                DateOfBirth = new DateOnly(1996, 1, 17),
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
                Addresses = [new PersonAddress { PersonId = "BB-0014", AddressLine = "50 Hollyfield Road", Locality = "Bridgewater Hollow", State = "VC", Postcode = "2210", ValidFrom = new DateOnly(2020, 3, 1), ValidTo = null }],
                PhysicalDescriptions = [new PhysicalDescription { PersonId = "BB-0014", ApproximateHeightCm = 163, EyeColour = "Hazel", HairColour = "Brown", ObservedOn = new DateOnly(2023, 2, 11) }],
            },

            // BB-0015 — minimal/incomplete record: no DOB, no address, no physical
            // description, no alias.
            new()
            {
                PersonId = "BB-0015",
                GivenName = "Thomas",
                FamilyName = "Harrington",
                DateOfBirth = null,
                RecordStatus = RecordStatus.Active,
                CreatedAtUtc = NextCreatedAt(),
                UpdatedAtUtc = SeedEpoch,
            },
        };

        return people;
    }
}
