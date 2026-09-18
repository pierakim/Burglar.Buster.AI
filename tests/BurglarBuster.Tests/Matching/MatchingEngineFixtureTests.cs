using BurglarBuster.Core.Matching;
using BurglarBuster.Infrastructure.Matching;
using BurglarBuster.Infrastructure.Seeding;
using BurglarBuster.Tests.TestSupport;

namespace BurglarBuster.Tests.Matching;

/// <summary>
/// Proves the Milestone 3 exit criterion: known fixtures reproducibly produce every
/// required outcome (strong, ambiguous, no-match, insufficient, conflicting,
/// duplicate) through the real pipeline — DB retrieval (PersonCandidateRepository) +
/// pure scoring (MatchingEngine) — with no LLM involved. See
/// docs/fixture-scenarios.md for what each fixture ID represents.
/// </summary>
public class MatchingEngineFixtureTests : IDisposable
{
    private static readonly DateOnly ReferenceDate = new(2024, 6, 1);

    private readonly SqliteTestDatabase _database = new();
    private readonly MatchingService _service;

    public MatchingEngineFixtureTests()
    {
        _service = new MatchingService(new PersonCandidateRepository(_database.Context));
    }

    [Fact]
    public async Task Unique_exact_details_produce_strong_candidate()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var criteria = new SearchCriteria
        {
            GivenName = "Daniel",
            FamilyName = "Miller",
            DateOfBirth = new DateOnly(1989, 4, 12),
            Locality = "Silverbrook",
            ApproximateHeightCm = 180,
            EyeColour = "Brown",
            HairColour = "Brown",
        };

        var result = await _service.SearchAsync(criteria, ReferenceDate, CancellationToken.None);

        Assert.Equal(MatchOutcome.StrongCandidate, result.Outcome);
        Assert.Equal("BB-0001", result.Candidates[0].PersonId);
    }

    [Fact]
    public async Task Same_surname_and_approximate_age_and_locality_are_ambiguous()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var criteria = new SearchCriteria
        {
            FamilyName = "Ellison",
            ApproximateAge = 40,
            Locality = "Stonebridge",
        };

        var result = await _service.SearchAsync(criteria, ReferenceDate, CancellationToken.None);

        Assert.Equal(MatchOutcome.Ambiguous, result.Outcome);
        var topIds = result.Candidates.Take(2).Select(c => c.PersonId).ToList();
        Assert.Contains("BB-0010", topIds);
        Assert.Contains("BB-0011", topIds);
    }

    [Fact]
    public async Task Unrecognized_description_produces_no_match()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        // Deliberately invented, unusually long name/locality: edit distance is at
        // least the length difference from any pool entry, so this can never
        // accidentally collide with a fixture or a randomly generated bulk record —
        // see docs/fixture-scenarios.md.
        var criteria = new SearchCriteria
        {
            GivenName = "Persephone",
            FamilyName = "Winterbourne",
            Locality = "Hollowmere Ridge",
        };

        var result = await _service.SearchAsync(criteria, ReferenceDate, CancellationToken.None);

        Assert.Equal(MatchOutcome.NoMatch, result.Outcome);
        Assert.Empty(result.Candidates);
    }

    [Fact]
    public async Task Appearance_only_description_is_insufficient_information()
    {
        // No DB seeding needed — insufficiency is decided from the criteria alone.
        var criteria = new SearchCriteria { EyeColour = "Brown", HairColour = "Black" };

        var result = await _service.SearchAsync(criteria, ReferenceDate, CancellationToken.None);

        Assert.Equal(MatchOutcome.InsufficientInformation, result.Outcome);
        Assert.Empty(result.Candidates);
    }

    [Fact]
    public async Task Internally_inconsistent_record_produces_conflicting_information()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var criteria = new SearchCriteria
        {
            GivenName = "William",
            FamilyName = "Foster",
            DateOfBirth = new DateOnly(1990, 12, 25),
        };

        var result = await _service.SearchAsync(criteria, ReferenceDate, CancellationToken.None);

        Assert.Equal(MatchOutcome.ConflictingInformation, result.Outcome);
        Assert.Equal("BB-0012", result.Candidates[0].PersonId);
    }

    [Fact]
    public async Task Near_identical_records_produce_potential_duplicate()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var criteria = new SearchCriteria
        {
            FamilyName = "Grant",
            DateOfBirth = new DateOnly(1996, 1, 17),
            AddressFragment = "Hollyfield",
        };

        var result = await _service.SearchAsync(criteria, ReferenceDate, CancellationToken.None);

        Assert.Equal(MatchOutcome.PotentialDuplicate, result.Outcome);
        var topIds = result.Candidates.Take(2).Select(c => c.PersonId).ToList();
        Assert.Contains("BB-0013", topIds);
        Assert.Contains("BB-0014", topIds);
    }

    [Fact]
    public async Task Identical_criteria_produce_identical_results_on_repeated_runs()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);
        var criteria = new SearchCriteria { GivenName = "Daniel", FamilyName = "Miller", DateOfBirth = new DateOnly(1989, 4, 12) };

        var first = await _service.SearchAsync(criteria, ReferenceDate, CancellationToken.None);
        var second = await _service.SearchAsync(criteria, ReferenceDate, CancellationToken.None);

        Assert.Equal(first.Outcome, second.Outcome);
        Assert.Equal(
            first.Candidates.Select(c => (c.PersonId, c.Score, c.Rank)),
            second.Candidates.Select(c => (c.PersonId, c.Score, c.Rank)));
    }

    public void Dispose() => _database.Dispose();
}
