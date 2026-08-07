using DevicePulse.Api.Contracts;
using DevicePulse.Api.Data;
using DevicePulse.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DevicePulse.Api.Services;

public sealed class DashboardService(DevicePulseDbContext dbContext) : IDashboardService
{
    public async Task<DashboardSummaryResponse> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var equipmentCount = await dbContext.Equipments
            .AsNoTracking()
            .CountAsync(cancellationToken);
        var readingCounts = await dbContext.EquipmentReadings
            .AsNoTracking()
            .GroupBy(reading => reading.Source)
            .Select(group => new { Source = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Source, item => item.Count, cancellationToken);

        readingCounts.TryGetValue(ReadingSource.Initial, out var initialCount);
        readingCounts.TryGetValue(ReadingSource.Manual, out var manualCount);
        readingCounts.TryGetValue(ReadingSource.Autopilot, out var autopilotCount);

        return new DashboardSummaryResponse(
            equipmentCount,
            initialCount + manualCount + autopilotCount,
            initialCount,
            manualCount,
            autopilotCount);
    }
}
