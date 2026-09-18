using BurglarBuster.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BurglarBuster.Tests.TestSupport;

/// <summary>
/// A fresh, migrated, in-memory SQLite database for one test. Uses a real SQLite
/// connection (not the EF Core InMemory provider) so migrations and relational
/// behaviour are actually exercised — see spec §18's "temporary SQLite database"
/// component-test guidance. Dispose per test to get full isolation.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public BurglarBusterDbContext Context { get; }

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<BurglarBusterDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new BurglarBusterDbContext(options);
        Context.Database.Migrate();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
