using BurglarBuster.Infrastructure.Seeding;

namespace BurglarBuster.Tests.Seeding;

public class BulkPersonGeneratorTests
{
    [Fact]
    public void Same_seed_produces_identical_output()
    {
        var first = BulkPersonGenerator.Generate(startNumber: 100, count: 50, randomSeed: 20260101);
        var second = BulkPersonGenerator.Generate(startNumber: 100, count: 50, randomSeed: 20260101);

        Assert.Equal(first.Count, second.Count);
        for (var i = 0; i < first.Count; i++)
        {
            Assert.Equal(first[i].PersonId, second[i].PersonId);
            Assert.Equal(first[i].GivenName, second[i].GivenName);
            Assert.Equal(first[i].FamilyName, second[i].FamilyName);
            Assert.Equal(first[i].DateOfBirth, second[i].DateOfBirth);
            Assert.Equal(first[i].RecordStatus, second[i].RecordStatus);
            Assert.Equal(first[i].Addresses.Single().Postcode, second[i].Addresses.Single().Postcode);
            Assert.Equal(first[i].PhysicalDescriptions.Single().ApproximateHeightCm, second[i].PhysicalDescriptions.Single().ApproximateHeightCm);
        }
    }

    [Fact]
    public void Different_seed_produces_different_output()
    {
        var first = BulkPersonGenerator.Generate(startNumber: 100, count: 50, randomSeed: 20260101);
        var second = BulkPersonGenerator.Generate(startNumber: 100, count: 50, randomSeed: 999);

        Assert.NotEqual(
            first.Select(p => (p.GivenName, p.FamilyName, p.DateOfBirth)),
            second.Select(p => (p.GivenName, p.FamilyName, p.DateOfBirth)));
    }

    [Fact]
    public void Generates_sequential_ids_from_start_number()
    {
        var people = BulkPersonGenerator.Generate(startNumber: 100, count: 5, randomSeed: 1);

        Assert.Equal(["BB-0100", "BB-0101", "BB-0102", "BB-0103", "BB-0104"], people.Select(p => p.PersonId));
    }
}
