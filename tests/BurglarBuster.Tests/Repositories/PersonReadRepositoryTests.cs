using BurglarBuster.Infrastructure.Repositories;
using BurglarBuster.Infrastructure.Seeding;
using BurglarBuster.Tests.TestSupport;

namespace BurglarBuster.Tests.Repositories;

public class PersonReadRepositoryTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    [Fact]
    public async Task GetByIdAsync_returns_the_person_with_related_evidence()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);
        var repository = new PersonReadRepository(_database.Context);

        var person = await repository.GetByIdAsync("BB-0005", CancellationToken.None);

        Assert.NotNull(person);
        Assert.Equal("Robert", person.GivenName);
        Assert.Single(person.Aliases);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_for_an_unknown_id()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);
        var repository = new PersonReadRepository(_database.Context);

        var person = await repository.GetByIdAsync("BB-9999", CancellationToken.None);

        Assert.Null(person);
    }

    public void Dispose() => _database.Dispose();
}
