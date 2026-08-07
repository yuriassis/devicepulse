using DevicePulse.Api.Data;
using DevicePulse.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DevicePulse.Api.Services;

public sealed class AutopilotReadingGenerator(DevicePulseDbContext dbContext) : IAutopilotReadingGenerator
{
    public async Task<int> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var equipments = await dbContext.Equipments.ToListAsync(cancellationToken);
        var recordedAt = DateTime.UtcNow;

        foreach (var equipment in equipments)
        {
            var value = equipment.MinimumValue
                + (Random.Shared.NextDouble() * (equipment.MaximumValue - equipment.MinimumValue));
            equipment.CurrentValue = value;
            equipment.UpdatedAt = recordedAt;
            dbContext.EquipmentReadings.Add(new EquipmentReading
            {
                EquipmentId = equipment.Id,
                Value = value,
                Source = ReadingSource.Autopilot,
                RecordedAt = recordedAt
            });
        }

        if (equipments.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return equipments.Count;
    }
}
