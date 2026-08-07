using DevicePulse.Api.Contracts;
using DevicePulse.Api.Models;
using DevicePulse.Api.Services;
using DevicePulse.Tests.Support;

namespace DevicePulse.Tests.Services;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_ReturnsEquipmentAndReadingCountsBySource()
    {
        await using var database = await TestDatabase.CreateAsync();
        var equipmentService = new EquipmentService(database.Context);
        var dashboardService = new DashboardService(database.Context);
        var motor = await equipmentService.CreateAsync(new CreateEquipmentRequest("Motor", 10, 20, 12));
        await equipmentService.CreateAsync(new CreateEquipmentRequest("Pump", 20, 40, 25));
        await equipmentService.CreateReadingAsync(
            motor.Id,
            new CreateReadingRequest(30, ReadingSource.Manual));
        await equipmentService.CreateReadingAsync(
            motor.Id,
            new CreateReadingRequest(15, ReadingSource.Autopilot));

        var summary = await dashboardService.GetSummaryAsync();

        Assert.Equal(2, summary.EquipmentCount);
        Assert.Equal(4, summary.ReadingCount);
        Assert.Equal(2, summary.InitialReadingCount);
        Assert.Equal(1, summary.ManualReadingCount);
        Assert.Equal(1, summary.AutopilotReadingCount);
    }
}
