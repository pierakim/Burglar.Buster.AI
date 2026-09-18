using BurglarBuster.Core.Entities;
using BurglarBuster.Core.Matching;

namespace BurglarBuster.Tests.Matching;

public class DuplicateDetectorTests
{
    [Fact]
    public void Same_dob_similar_name_and_shared_address_is_a_likely_duplicate()
    {
        var a = CreatePerson("BB-A", "Sophia", "Grant", new DateOnly(1996, 1, 17), "50 Hollyfield Road");
        var b = CreatePerson("BB-B", "Sofia", "Grant", new DateOnly(1996, 1, 17), "50 Hollyfield Road");

        Assert.True(DuplicateDetector.AreLikelyDuplicates(a, b));
    }

    [Fact]
    public void Different_dob_is_not_a_duplicate_even_with_similar_name_and_address()
    {
        var a = CreatePerson("BB-A", "Sophia", "Grant", new DateOnly(1996, 1, 17), "50 Hollyfield Road");
        var b = CreatePerson("BB-B", "Sofia", "Grant", new DateOnly(1990, 5, 1), "50 Hollyfield Road");

        Assert.False(DuplicateDetector.AreLikelyDuplicates(a, b));
    }

    [Fact]
    public void Same_dob_and_similar_name_without_a_shared_address_is_not_a_duplicate()
    {
        var a = CreatePerson("BB-A", "Sophia", "Grant", new DateOnly(1996, 1, 17), "1 Somewhere Street");
        var b = CreatePerson("BB-B", "Sofia", "Grant", new DateOnly(1996, 1, 17), "99 Elsewhere Road");

        Assert.False(DuplicateDetector.AreLikelyDuplicates(a, b));
    }

    [Fact]
    public void A_person_is_never_a_duplicate_of_itself()
    {
        var a = CreatePerson("BB-A", "Sophia", "Grant", new DateOnly(1996, 1, 17), "50 Hollyfield Road");

        Assert.False(DuplicateDetector.AreLikelyDuplicates(a, a));
    }

    private static Person CreatePerson(string id, string given, string family, DateOnly dob, string addressLine)
    {
        var person = new Person
        {
            PersonId = id,
            GivenName = given,
            FamilyName = family,
            DateOfBirth = dob,
            RecordStatus = RecordStatus.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
        };
        person.Addresses.Add(new PersonAddress { PersonId = id, AddressLine = addressLine, Locality = "Test", State = "TS", Postcode = "0000" });
        return person;
    }
}
