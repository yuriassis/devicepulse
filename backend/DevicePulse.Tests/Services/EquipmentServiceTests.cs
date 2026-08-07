using DevicePulse.Api.Contracts;
using DevicePulse.Api.Data;
using DevicePulse.Api.Exceptions;
using DevicePulse.Api.Models;
using DevicePulse.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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

    private sealed class TestDatabase : IAsyncDisposable
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
}
