using BurglarBuster.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BurglarBuster.Infrastructure.Repositories;

public sealed class PersonReadRepository(BurglarBusterDbContext context) : IPersonReadRepository
{
    public Task<Person?> GetByIdAsync(string personId, CancellationToken cancellationToken)
    {
        return context.People
            .AsNoTracking()
            .Include(p => p.Aliases)
            .Include(p => p.Addresses)
            .Include(p => p.PhysicalDescriptions)
            .Include(p => p.CaseReferences)
            .FirstOrDefaultAsync(p => p.PersonId == personId, cancellationToken);
    }

    public async Task<IReadOnlyList<Person>> GetByIdsAsync(IReadOnlyList<string> personIds, CancellationToken cancellationToken)
    {
        return await context.People
            .AsNoTracking()
            .Include(p => p.Aliases)
            .Include(p => p.Addresses)
            .Include(p => p.PhysicalDescriptions)
            .Where(p => personIds.Contains(p.PersonId))
            .ToListAsync(cancellationToken);
    }
}
