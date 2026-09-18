using BurglarBuster.Core.Entities;
using BurglarBuster.Core.Matching;
using Microsoft.EntityFrameworkCore;

namespace BurglarBuster.Infrastructure.Matching;

/// <summary>
/// Stage 1 retrieval: a broad-recall, parameterized query bounded to
/// <see cref="MatchingPolicy.MaxCandidatePoolSize" /> rows. Deliberately generous
/// (short name prefixes rather than exact matches) so that stage 2's precise
/// scoring — including fuzzy name matching — has a fair pool to work with; e.g. a
/// "Miller" search must still surface a "Millar" record misspelled by one letter.
/// Never reads CaseReferences — case history must not affect identity similarity
/// (spec §9).
/// </summary>
public sealed class PersonCandidateRepository(BurglarBusterDbContext context) : IPersonCandidateRepository
{
    public async Task<IReadOnlyList<Person>> GetCandidatePoolAsync(SearchCriteria criteria, CancellationToken cancellationToken)
    {
        var familyPattern = ToPrefixPattern(criteria.FamilyName);
        var givenPattern = ToPrefixPattern(criteria.GivenName);
        var fullNamePattern = ToPrefixPattern(criteria.FullName);
        var aliasPattern = ToPrefixPattern(criteria.AliasName);
        var locality = string.IsNullOrWhiteSpace(criteria.Locality) ? null : criteria.Locality.Trim();
        var postcode = string.IsNullOrWhiteSpace(criteria.Postcode) ? null : criteria.Postcode.Trim();
        var addressFragmentPattern = string.IsNullOrWhiteSpace(criteria.AddressFragment) ? null : $"%{criteria.AddressFragment.Trim()}%";

        var hasAnyHint = familyPattern != null || givenPattern != null || fullNamePattern != null
            || aliasPattern != null || locality != null || postcode != null || addressFragmentPattern != null;

        if (!hasAnyHint)
        {
            return [];
        }

        var query = context.People
            .AsNoTracking()
            .Include(p => p.Aliases)
            .Include(p => p.Addresses)
            .Include(p => p.PhysicalDescriptions)
            .Where(p =>
                (familyPattern != null && EF.Functions.Like(p.FamilyName, familyPattern))
                || (givenPattern != null && EF.Functions.Like(p.GivenName, givenPattern))
                || (fullNamePattern != null && (EF.Functions.Like(p.GivenName, fullNamePattern) || EF.Functions.Like(p.FamilyName, fullNamePattern)))
                || (aliasPattern != null && p.Aliases.Any(a => EF.Functions.Like(a.FullName, aliasPattern)))
                || (locality != null && p.Addresses.Any(a => a.Locality == locality))
                || (postcode != null && p.Addresses.Any(a => a.Postcode == postcode))
                || (addressFragmentPattern != null && p.Addresses.Any(a => EF.Functions.Like(a.AddressLine, addressFragmentPattern))))
            .Take(MatchingPolicy.MaxCandidatePoolSize);

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// A raw (unnormalized) prefix pattern for the SQL LIKE against the actual column
    /// value — deliberately not using Core's NameNormalizer, which strips punctuation
    /// like apostrophes that the raw "O'Brien"-style column values still contain.
    /// SQLite's LIKE is already case-insensitive for ASCII, which is all this needs.
    /// </summary>
    private static string? ToPrefixPattern(string? value, int length = 4)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        var prefix = trimmed.Length <= length ? trimmed : trimmed[..length];
        return $"{prefix}%";
    }
}
