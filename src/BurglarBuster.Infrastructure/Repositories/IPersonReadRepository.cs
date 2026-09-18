using BurglarBuster.Core.Entities;

namespace BurglarBuster.Infrastructure.Repositories;

/// <summary>
/// Read-only access to person records. Deliberately has no Add/Update/Remove members —
/// person data is read-only by design after seeding (see CLAUDE.md).
/// </summary>
public interface IPersonReadRepository
{
    /// <summary>Returns the person (with all related evidence) or null if the ID is unknown.</summary>
    Task<Person?> GetByIdAsync(string personId, CancellationToken cancellationToken);

    /// <summary>Returns whichever of the given IDs exist — silently omits unknown ones
    /// rather than throwing, so callers can report them explicitly if useful.</summary>
    Task<IReadOnlyList<Person>> GetByIdsAsync(IReadOnlyList<string> personIds, CancellationToken cancellationToken);
}
