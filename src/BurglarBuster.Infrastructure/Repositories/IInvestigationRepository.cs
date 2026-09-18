using BurglarBuster.Core.Entities;

namespace BurglarBuster.Infrastructure.Repositories;

public interface IInvestigationRepository
{
    Task SaveAsync(Investigation investigation, CancellationToken cancellationToken);

    Task<Investigation?> GetByIdAsync(string investigationId, CancellationToken cancellationToken);
}
