using DevicePulse.Api.Data;
using DevicePulse.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DevicePulse.Tests.Integration;

public sealed class SqlitePersistenceTests
{
    [Fact]
    public async Task MigrationsPreserveDataAcrossConnections()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"devicepulse-persistence-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<DevicePulseDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        try
        {
            await using (var firstContext = new DevicePulseDbContext(options))
            {
                await firstContext.Database.MigrateAsync();
                firstContext.Equipments.Add(new Equipment
                {
                    Name = "Persistent sensor",
                    MinimumValue = 0,
                    MaximumValue = 100,
                    CurrentValue = 42,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await firstContext.SaveChangesAsync();
            }

            await using var secondContext = new DevicePulseDbContext(options);
            await secondContext.Database.MigrateAsync();
            var equipment = await secondContext.Equipments.SingleAsync();

            Assert.Equal("Persistent sensor", equipment.Name);
            Assert.Equal(42, equipment.CurrentValue);
        }
        finally
        {
            if (File.Exists(databasePath)) File.Delete(databasePath);
        }
    }
}
