using BurglarBuster.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BurglarBuster.Infrastructure.Repositories;

public sealed class InvestigationRepository(BurglarBusterDbContext context) : IInvestigationRepository
{
    public async Task SaveAsync(Investigation investigation, CancellationToken cancellationToken)
    {
        context.Investigations.Add(investigation);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<Investigation?> GetByIdAsync(string investigationId, CancellationToken cancellationToken)
    {
        return context.Investigations
            .AsNoTracking()
            .Include(i => i.Candidates)
            .FirstOrDefaultAsync(i => i.InvestigationId == investigationId, cancellationToken);
    }
}
