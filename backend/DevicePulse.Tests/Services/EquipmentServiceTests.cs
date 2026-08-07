using DevicePulse.Api.Contracts;
using DevicePulse.Api.Exceptions;
using DevicePulse.Api.Models;
using DevicePulse.Api.Services;
using Microsoft.EntityFrameworkCore;
using DevicePulse.Tests.Support;

namespace DevicePulse.Tests.Services;

public sealed class EquipmentServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesNormalizedEquipment()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);

        var result = await service.CreateAsync(new CreateEquipmentRequest("  Motor temperature  ", 60, 90, 72));

        Assert.True(result.Id > 0);
        Assert.Equal("Motor temperature", result.Name);
        Assert.Equal(60, result.MinimumValue);
        Assert.Equal(90, result.MaximumValue);
        Assert.Equal(72, result.CurrentValue);
        Assert.Equal(ReadingSource.Initial, result.LastReadingSource);
        Assert.Equal(result.CreatedAt, result.UpdatedAt);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesInitialReading()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);

        var equipment = await service.CreateAsync(new CreateEquipmentRequest("Hydraulic pressure", 20, 45, 32));
        var reading = await database.Context.EquipmentReadings.SingleAsync();

        Assert.Equal(equipment.Id, reading.EquipmentId);
        Assert.Equal(32, reading.Value);
        Assert.Equal(ReadingSource.Initial, reading.Source);
        Assert.Equal(equipment.CreatedAt, reading.RecordedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithoutName_ThrowsValidationException(string? name)
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);

        var exception = await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.CreateAsync(new CreateEquipmentRequest(name, 0, 10, 5)));

        Assert.Equal("Name is required.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ThrowsDuplicateException()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);
        var request = new CreateEquipmentRequest("Conveyor speed", 0, 120, 65);
        await service.CreateAsync(request);

        await Assert.ThrowsAsync<DuplicateEquipmentNameException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WithDifferentlyCasedDuplicateName_ThrowsDuplicateException()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);
        await service.CreateAsync(new CreateEquipmentRequest("Main Motor", 0, 100, 50));

        await Assert.ThrowsAsync<DuplicateEquipmentNameException>(() =>
            service.CreateAsync(new CreateEquipmentRequest("main motor", 0, 100, 50)));
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(11, 10)]
    public async Task CreateAsync_WithInvalidAutomaticRange_ThrowsValidationException(
        double minimumValue,
        double maximumValue)
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);

        var exception = await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.CreateAsync(new CreateEquipmentRequest("Sensor", minimumValue, maximumValue, 1000)));

        Assert.Equal("MinimumValue must be less than MaximumValue.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithNonFiniteValue_ThrowsValidationException()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.CreateAsync(new CreateEquipmentRequest("Sensor", double.NaN, 10, 5)));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEquipmentsOrderedByName()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);
        await service.CreateAsync(new CreateEquipmentRequest("Zulu", 0, 10, 5));
        await service.CreateAsync(new CreateEquipmentRequest("Alpha", 0, 10, 5));

        var result = await service.GetAllAsync();

        Assert.Collection(
            result,
            equipment => Assert.Equal("Alpha", equipment.Name),
            equipment => Assert.Equal("Zulu", equipment.Name));
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_UpdatesConfigurationButKeepsCurrentValue()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);
        var created = await service.CreateAsync(new CreateEquipmentRequest("Motor", 10, 20, 30));

        var updated = await service.UpdateAsync(
            created.Id,
            new UpdateEquipmentRequest(" Main motor ", 5, 25));

        Assert.NotNull(updated);
        Assert.Equal("Main motor", updated.Name);
        Assert.Equal(5, updated.MinimumValue);
        Assert.Equal(25, updated.MaximumValue);
        Assert.Equal(30, updated.CurrentValue);
        Assert.True(updated.UpdatedAt >= created.UpdatedAt);
    }

    [Theory]
    [InlineData(15, ReadingSource.Manual)]
    [InlineData(150, ReadingSource.Manual)]
    [InlineData(17.5, ReadingSource.Autopilot)]
    public async Task CreateReadingAsync_WithAllowedReading_UpdatesCurrentValueAndHistory(
        double value,
        ReadingSource source)
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);
        var equipment = await service.CreateAsync(new CreateEquipmentRequest("Motor", 10, 20, 12));

        var reading = await service.CreateReadingAsync(
            equipment.Id,
            new CreateReadingRequest(value, source));
        var persistedEquipment = await database.Context.Equipments.FindAsync(equipment.Id);
        var readings = await database.Context.EquipmentReadings
            .Where(item => item.EquipmentId == equipment.Id)
            .OrderBy(item => item.Id)
            .ToListAsync();

        Assert.NotNull(reading);
        Assert.Equal(value, persistedEquipment!.CurrentValue);
        Assert.Equal(value, reading.Value);
        Assert.Equal(source, reading.Source);
        Assert.Equal(2, readings.Count);
        Assert.Equal(ReadingSource.Initial, readings[0].Source);
        Assert.Equal(source, readings[1].Source);
    }

    [Fact]
    public async Task CreateReadingAsync_WithInitialSource_ThrowsValidationException()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);
        var equipment = await service.CreateAsync(new CreateEquipmentRequest("Motor", 10, 20, 12));

        var exception = await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.CreateReadingAsync(
                equipment.Id,
                new CreateReadingRequest(14, ReadingSource.Initial)));

        Assert.Equal("Source must be Manual or Autopilot.", exception.Message);
    }

    [Fact]
    public async Task CreateReadingAsync_WithUnknownEquipment_ReturnsNull()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);

        var reading = await service.CreateReadingAsync(
            999,
            new CreateReadingRequest(14, ReadingSource.Manual));

        Assert.Null(reading);
    }

    [Fact]
    public async Task GetReadingsAsync_ReturnsNewestReadingsWithinLimit()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);
        var equipment = await service.CreateAsync(new CreateEquipmentRequest("Motor", 10, 20, 12));
        await service.CreateReadingAsync(equipment.Id, new CreateReadingRequest(13, ReadingSource.Manual));
        await service.CreateReadingAsync(equipment.Id, new CreateReadingRequest(14, ReadingSource.Autopilot));

        var readings = await service.GetReadingsAsync(equipment.Id, 2);

        Assert.NotNull(readings);
        Assert.Collection(
            readings,
            reading => Assert.Equal(14, reading.Value),
            reading => Assert.Equal(13, reading.Value));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetReadingsAsync_WithInvalidLimit_ThrowsValidationException(int limit)
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.GetReadingsAsync(1, limit));
    }

    [Fact]
    public async Task DeleteAsync_RemovesEquipmentAndItsReadings()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new EquipmentService(database.Context);
        var equipment = await service.CreateAsync(new CreateEquipmentRequest("Motor", 10, 20, 12));
        await service.CreateReadingAsync(equipment.Id, new CreateReadingRequest(13, ReadingSource.Manual));

        var deleted = await service.DeleteAsync(equipment.Id);

        Assert.True(deleted);
        Assert.Empty(await database.Context.Equipments.ToListAsync());
        Assert.Empty(await database.Context.EquipmentReadings.ToListAsync());
    }
}
