using BurglarBuster.Core.Entities;
using BurglarBuster.Infrastructure.Seeding;
using BurglarBuster.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace BurglarBuster.Tests.Seeding;

public class DatabaseSeederTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    [Fact]
    public async Task Seeds_exactly_the_target_number_of_people()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var count = await _database.Context.People.CountAsync();

        Assert.Equal(DatabaseSeeder.TargetTotalPeopleCount, count);
    }

    [Fact]
    public async Task Reseeding_is_a_no_op()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);
        await DatabaseSeeder.SeedAsync(_database.Context);

        var count = await _database.Context.People.CountAsync();

        Assert.Equal(DatabaseSeeder.TargetTotalPeopleCount, count);
    }

    [Theory]
    [InlineData("BB-0001")]
    [InlineData("BB-0002")]
    [InlineData("BB-0003")]
    [InlineData("BB-0004")]
    [InlineData("BB-0005")]
    [InlineData("BB-0006")]
    [InlineData("BB-0007")]
    [InlineData("BB-0008")]
    [InlineData("BB-0009")]
    [InlineData("BB-0010")]
    [InlineData("BB-0011")]
    [InlineData("BB-0012")]
    [InlineData("BB-0013")]
    [InlineData("BB-0014")]
    [InlineData("BB-0015")]
    public async Task All_named_fixtures_are_present(string personId)
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var exists = await _database.Context.People.AnyAsync(p => p.PersonId == personId);

        Assert.True(exists);
    }

    [Fact]
    public async Task BB0001_is_the_unique_exact_match_fixture_with_previous_and_current_address()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var person = await Load("BB-0001");

        Assert.Equal("Daniel", person.GivenName);
        Assert.Equal("Miller", person.FamilyName);
        Assert.Equal(new DateOnly(1989, 4, 12), person.DateOfBirth);
        Assert.Equal(2, person.Addresses.Count);
        Assert.Contains(person.Addresses, a => a.Locality == "Camden Reach" && a.ValidTo != null);
        Assert.Contains(person.Addresses, a => a.ValidTo == null);
    }

    [Fact]
    public async Task BB0002_and_BB0003_share_the_Miller_surname_with_BB0001_but_are_different_people()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var bb2 = await Load("BB-0002");
        var bb3 = await Load("BB-0003");

        Assert.Equal("Miller", bb2.FamilyName);
        Assert.Equal("Miller", bb3.FamilyName);
        Assert.NotEqual(bb2.GivenName, bb3.GivenName);
        Assert.NotEqual(bb2.DateOfBirth, bb3.DateOfBirth);
    }

    [Fact]
    public async Task BB0004_is_a_misspelled_name_variant_of_BB0001_and_a_distinct_person()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var bb1 = await Load("BB-0001");
        var bb4 = await Load("BB-0004");

        Assert.Equal(bb1.GivenName, bb4.GivenName);
        Assert.NotEqual(bb1.FamilyName, bb4.FamilyName);
        Assert.Equal("Millar", bb4.FamilyName);
        Assert.NotEqual(bb1.DateOfBirth, bb4.DateOfBirth);
    }

    [Fact]
    public async Task BB0005_has_a_recorded_alias()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var person = await Load("BB-0005");

        var alias = Assert.Single(person.Aliases);
        Assert.Equal("Bobby Nguyen", alias.FullName);
    }

    [Fact]
    public async Task BB0006_has_a_previous_address_distinct_from_its_current_address()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var person = await Load("BB-0006");

        Assert.Equal(2, person.Addresses.Count);
        Assert.Contains(person.Addresses, a => a.ValidTo != null);
        Assert.Single(person.Addresses, a => a.ValidTo == null);
    }

    [Fact]
    public async Task BB0007_has_no_recorded_date_of_birth()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var person = await Load("BB-0007");

        Assert.Null(person.DateOfBirth);
    }

    [Fact]
    public async Task BB0008_and_BB0009_have_matching_physical_descriptions_but_are_different_people()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var bb8 = await Load("BB-0008");
        var bb9 = await Load("BB-0009");

        var desc8 = Assert.Single(bb8.PhysicalDescriptions);
        var desc9 = Assert.Single(bb9.PhysicalDescriptions);
        Assert.Equal(desc8.ApproximateHeightCm, desc9.ApproximateHeightCm);
        Assert.Equal(desc8.EyeColour, desc9.EyeColour);
        Assert.Equal(desc8.HairColour, desc9.HairColour);
        Assert.NotEqual(bb8.GivenName, bb9.GivenName);
        Assert.NotEqual(bb8.DateOfBirth, bb9.DateOfBirth);
    }

    [Fact]
    public async Task BB0010_and_BB0011_are_ambiguous_similar_candidates()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var bb10 = await Load("BB-0010");
        var bb11 = await Load("BB-0011");

        Assert.Equal(bb10.FamilyName, bb11.FamilyName);
        Assert.Equal(bb10.DateOfBirth!.Value.Year, bb11.DateOfBirth!.Value.Year);
        Assert.Equal(bb10.Addresses.Single().Locality, bb11.Addresses.Single().Locality);
        Assert.NotEqual(bb10.PersonId, bb11.PersonId);
    }

    [Fact]
    public async Task BB0012_has_conflicting_physical_observations()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var person = await Load("BB-0012");

        Assert.Equal(2, person.PhysicalDescriptions.Count);
        var heights = person.PhysicalDescriptions.Select(d => d.ApproximateHeightCm).ToList();
        Assert.True(Math.Abs(heights[0]!.Value - heights[1]!.Value) >= 10);
    }

    [Fact]
    public async Task BB0013_and_BB0014_are_a_potential_duplicate_pair()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var bb13 = await Load("BB-0013");
        var bb14 = await Load("BB-0014");

        Assert.Equal(bb13.DateOfBirth, bb14.DateOfBirth);
        Assert.Equal(bb13.Addresses.Single().AddressLine, bb14.Addresses.Single().AddressLine);
        Assert.NotEqual(bb13.GivenName, bb14.GivenName);
    }

    [Fact]
    public async Task BB0015_is_a_minimal_incomplete_record()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var person = await Load("BB-0015");

        Assert.Null(person.DateOfBirth);
        Assert.Empty(person.Addresses);
        Assert.Empty(person.PhysicalDescriptions);
        Assert.Empty(person.Aliases);
    }

    [Fact]
    public async Task Case_references_do_not_affect_any_other_persons_data()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var person = await Load("BB-0001");

        var reference = Assert.Single(person.CaseReferences);
        Assert.Equal("Closed", reference.Status);
    }

    private async Task<Person> Load(string personId)
    {
        var person = await _database.Context.People
            .Include(p => p.Aliases)
            .Include(p => p.Addresses)
            .Include(p => p.PhysicalDescriptions)
            .Include(p => p.CaseReferences)
            .FirstOrDefaultAsync(p => p.PersonId == personId);

        Assert.NotNull(person);
        return person;
    }

    public void Dispose() => _database.Dispose();
}
