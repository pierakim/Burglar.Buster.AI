using BurglarBuster.Core.Entities;
using BurglarBuster.Core.Matching;

namespace BurglarBuster.Tests.Matching;

public class CandidateScorerTests
{
    private static readonly DateOnly ReferenceDate = new(2024, 6, 1);

    [Fact]
    public void Exact_name_and_dob_score_highly_and_match_two_independent_groups()
    {
        var person = CreatePerson("BB-TEST-1", "Daniel", "Miller", new DateOnly(1989, 4, 12));
        var criteria = new SearchCriteria { GivenName = "Daniel", FamilyName = "Miller", DateOfBirth = new DateOnly(1989, 4, 12) };

        var (score, evidence) = CandidateScorer.Evaluate(person, criteria, ReferenceDate);

        Assert.True(score >= MatchingPolicy.StrongCandidateMinScore);
        Assert.Equal(2, evidence.IndependentGroupsMatched);
        Assert.Contains("date of birth", evidence.MatchedEvidence);
        Assert.Empty(evidence.Conflicts);
    }

    [Fact]
    public void Misspelled_family_name_earns_partial_similar_credit_not_exact()
    {
        var person = CreatePerson("BB-TEST-2", "Daniel", "Millar", new DateOnly(1977, 2, 28));
        var criteria = new SearchCriteria { GivenName = "Daniel", FamilyName = "Miller" };

        var (score, evidence) = CandidateScorer.Evaluate(person, criteria, ReferenceDate);

        Assert.Equal(MatchingPolicy.GivenNameExactPoints + MatchingPolicy.FamilyNameSimilarPoints, score);
        Assert.Empty(evidence.Conflicts);
    }

    [Fact]
    public void Mismatched_date_of_birth_is_a_conflict_not_a_score()
    {
        var person = CreatePerson("BB-TEST-3", "Robert", "Miller", new DateOnly(1965, 11, 2));
        var criteria = new SearchCriteria { DateOfBirth = new DateOnly(1989, 4, 12) };

        var (score, evidence) = CandidateScorer.Evaluate(person, criteria, ReferenceDate);

        Assert.Equal(0, score);
        var conflict = Assert.Single(evidence.Conflicts);
        Assert.Contains("date of birth differs", conflict);
    }

    [Fact]
    public void Missing_date_of_birth_is_missing_evidence_not_a_conflict()
    {
        var person = CreatePerson("BB-TEST-4", "Michael", "Brooks", dateOfBirth: null);
        var criteria = new SearchCriteria { DateOfBirth = new DateOnly(1989, 4, 12) };

        var (_, evidence) = CandidateScorer.Evaluate(person, criteria, ReferenceDate);

        Assert.Empty(evidence.Conflicts);
        Assert.Contains("date of birth", evidence.MissingEvidence);
    }

    [Fact]
    public void Appearance_alone_cannot_reach_the_strong_threshold_or_count_as_an_independent_group()
    {
        var person = CreatePerson("BB-TEST-5", "Anyone", "Nobody", new DateOnly(1990, 1, 1));
        AddDescription(person, height: 180, eye: "Brown", hair: "Brown");
        var criteria = new SearchCriteria { ApproximateHeightCm = 180, EyeColour = "Brown", HairColour = "Brown" };

        var (score, evidence) = CandidateScorer.Evaluate(person, criteria, ReferenceDate);

        Assert.True(score < MatchingPolicy.StrongCandidateMinScore);
        Assert.Equal(0, evidence.IndependentGroupsMatched);
    }

    [Fact]
    public void Height_beyond_tolerance_is_a_conflict()
    {
        var person = CreatePerson("BB-TEST-6", "Anyone", "Nobody", new DateOnly(1990, 1, 1));
        AddDescription(person, height: 170, eye: null, hair: null);
        var criteria = new SearchCriteria { ApproximateHeightCm = 190 };

        var (_, evidence) = CandidateScorer.Evaluate(person, criteria, ReferenceDate);

        Assert.Contains(evidence.Conflicts, c => c.Contains("height differs"));
    }

    [Fact]
    public void Case_references_are_never_read_by_the_scorer()
    {
        // The Person type doesn't even expose case reference content to the scorer's
        // inputs beyond the navigation property, and no criteria field references
        // cases — this test documents that guarantee rather than exercising new code.
        var person = CreatePerson("BB-TEST-7", "Anyone", "Nobody", new DateOnly(1990, 1, 1));
        person.CaseReferences.Add(new CaseReference { PersonId = "BB-TEST-7", ReferenceNumber = "CR-1", Summary = "x", Date = new DateOnly(2020, 1, 1), Status = "Open" });

        var (score, _) = CandidateScorer.Evaluate(person, new SearchCriteria { FamilyName = "Nobody" }, ReferenceDate);

        Assert.Equal(MatchingPolicy.FamilyNameExactPoints, score);
    }

    private static Person CreatePerson(string id, string given, string family, DateOnly? dateOfBirth)
    {
        return new Person
        {
            PersonId = id,
            GivenName = given,
            FamilyName = family,
            DateOfBirth = dateOfBirth,
            RecordStatus = RecordStatus.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    private static void AddDescription(Person person, int? height, string? eye, string? hair)
    {
        person.PhysicalDescriptions.Add(new PhysicalDescription
        {
            PersonId = person.PersonId,
            ApproximateHeightCm = height,
            EyeColour = eye,
            HairColour = hair,
            ObservedOn = new DateOnly(2023, 1, 1),
        });
    }
}
