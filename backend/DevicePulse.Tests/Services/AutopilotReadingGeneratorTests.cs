using DevicePulse.Api.Models;
using DevicePulse.Api.Services;
using DevicePulse.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace DevicePulse.Tests.Services;

public sealed class AutopilotReadingGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_CreatesAutopilotReadingForEveryEquipment()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Context.Equipments.AddRange(
            CreateEquipment("Sensor A", 10, 20),
            CreateEquipment("Sensor B", -5, 5));
        await database.Context.SaveChangesAsync();
        var generator = new AutopilotReadingGenerator(database.Context);

        var generated = await generator.GenerateAsync();

        var equipments = await database.Context.Equipments.Include(item => item.Readings).ToListAsync();
        Assert.Equal(2, generated);
        Assert.All(equipments, equipment =>
        {
            var reading = Assert.Single(equipment.Readings);
            Assert.Equal(ReadingSource.Autopilot, reading.Source);
            Assert.InRange(reading.Value, equipment.MinimumValue, equipment.MaximumValue);
            Assert.Equal(reading.Value, equipment.CurrentValue);
        });
    }

    [Fact]
    public async Task GenerateAsync_WithNoEquipment_DoesNotCreateReadings()
    {
        await using var database = await TestDatabase.CreateAsync();
        var generator = new AutopilotReadingGenerator(database.Context);

        var generated = await generator.GenerateAsync();

        Assert.Equal(0, generated);
        Assert.Empty(database.Context.EquipmentReadings);
    }

    private static Equipment CreateEquipment(string name, double minimum, double maximum)
    {
        var now = DateTime.UtcNow;
        return new Equipment
        {
            Name = name,
            MinimumValue = minimum,
            MaximumValue = maximum,
            CurrentValue = minimum,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
