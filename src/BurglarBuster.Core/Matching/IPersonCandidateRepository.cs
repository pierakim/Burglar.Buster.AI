using BurglarBuster.Core.Entities;

namespace BurglarBuster.Core.Matching;

/// <summary>
/// Stage 1 of the two-stage design from spec §9: a bounded, parameterized-query
/// candidate pool. Read-only, like all person data access.
/// </summary>
public interface IPersonCandidateRepository
{
    Task<IReadOnlyList<Person>> GetCandidatePoolAsync(SearchCriteria criteria, CancellationToken cancellationToken);
}
