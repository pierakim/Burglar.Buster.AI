using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BurglarBuster.Infrastructure;

/// <summary>
/// Design-time factory used only by `dotnet ef` (migrations authoring). The runtime
/// app configures its own <see cref="DbContextOptions{TContext}" /> via DI in
/// BurglarBuster.Functions — this connection string is never used outside tooling.
/// </summary>
public sealed class BurglarBusterDbContextFactory : IDesignTimeDbContextFactory<BurglarBusterDbContext>
{
    public BurglarBusterDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BurglarBusterDbContext>();
        optionsBuilder.UseSqlite("Data Source=burglarbuster.design.db");
        return new BurglarBusterDbContext(optionsBuilder.Options);
    }
}
