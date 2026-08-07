using DevicePulse.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DevicePulse.Tests.Support;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection connection;

    private TestDatabase(SqliteConnection connection, DevicePulseDbContext context)
    {
        this.connection = connection;
        Context = context;
    }

    public DevicePulseDbContext Context { get; }

    public static async Task<TestDatabase> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<DevicePulseDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new DevicePulseDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return new TestDatabase(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await connection.DisposeAsync();
    }
}
