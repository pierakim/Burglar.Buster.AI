using Microsoft.EntityFrameworkCore;

namespace BurglarBuster.Infrastructure.Seeding;

/// <summary>
/// Deterministic seed generator: the same fixed <see cref="RandomSeed"/> always
/// produces the same ~1,000 people. Safe to call repeatedly — it's a no-op once the
/// database already has people (see BURGLAR_BUSTER_PROJECT_SPEC.md §8).
/// </summary>
public static class DatabaseSeeder
{
    public const int TargetTotalPeopleCount = 1000;
    public const int BulkPersonStartNumber = 100;
    public const int RandomSeed = 20260101;

    public static async Task SeedAsync(BurglarBusterDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.People.AnyAsync(cancellationToken))
        {
            return;
        }

        var fixtures = FixtureScenarios.GetAll();
        var bulkCount = TargetTotalPeopleCount - fixtures.Count;
        var bulk = BulkPersonGenerator.Generate(BulkPersonStartNumber, bulkCount, RandomSeed);

        context.People.AddRange(fixtures);
        context.People.AddRange(bulk);

        await context.SaveChangesAsync(cancellationToken);
    }
}
