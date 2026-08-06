using DevicePulse.Api.Contracts;
using DevicePulse.Api.Data;
using DevicePulse.Api.Exceptions;
using DevicePulse.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DevicePulse.Api.Services;

public sealed class EquipmentService(DevicePulseDbContext dbContext) : IEquipmentService
{
    public async Task<EquipmentResponse> CreateAsync(
        CreateEquipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = ValidateAndNormalize(request);

        if (await dbContext.Equipments.AnyAsync(
                equipment => equipment.Name == normalizedName,
                cancellationToken))
        {
            throw new DuplicateEquipmentNameException(normalizedName);
        }

        var now = DateTime.UtcNow;
        var equipment = new Equipment
        {
            Name = normalizedName,
            MinimumValue = request.MinimumValue,
            MaximumValue = request.MaximumValue,
            CurrentValue = request.CurrentValue,
            CreatedAt = now,
            UpdatedAt = now
        };

        equipment.Readings.Add(new EquipmentReading
        {
            Value = request.CurrentValue,
            Source = ReadingSource.Initial,
            RecordedAt = now
        });

        dbContext.Equipments.Add(equipment);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueNameViolation(exception))
        {
            throw new DuplicateEquipmentNameException(normalizedName);
        }

        return Map(equipment, ReadingSource.Initial);
    }

    public async Task<IReadOnlyList<EquipmentResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Equipments
            .AsNoTracking()
            .OrderBy(equipment => equipment.Name)
            .Select(equipment => new EquipmentResponse(
                equipment.Id,
                equipment.Name,
                equipment.MinimumValue,
                equipment.MaximumValue,
                equipment.CurrentValue,
                equipment.Readings
                    .OrderByDescending(reading => reading.RecordedAt)
                    .Select(reading => reading.Source)
                    .First(),
                equipment.CreatedAt,
                equipment.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public Task<EquipmentResponse?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Equipments
            .AsNoTracking()
            .Where(equipment => equipment.Id == id)
            .Select(equipment => new EquipmentResponse(
                equipment.Id,
                equipment.Name,
                equipment.MinimumValue,
                equipment.MaximumValue,
                equipment.CurrentValue,
                equipment.Readings
                    .OrderByDescending(reading => reading.RecordedAt)
                    .Select(reading => reading.Source)
                    .First(),
                equipment.CreatedAt,
                equipment.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static string ValidateAndNormalize(CreateEquipmentRequest request)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Name is required.");
        }

        if (name.Length > 100)
        {
            throw new DomainValidationException("Name must not exceed 100 characters.");
        }

        ValidateFinite(request.MinimumValue, nameof(request.MinimumValue));
        ValidateFinite(request.MaximumValue, nameof(request.MaximumValue));
        ValidateFinite(request.CurrentValue, nameof(request.CurrentValue));

        if (request.MinimumValue >= request.MaximumValue)
        {
            throw new DomainValidationException("MinimumValue must be less than MaximumValue.");
        }

        return name;
    }

    private static void ValidateFinite(double value, string fieldName)
    {
        if (!double.IsFinite(value))
        {
            throw new DomainValidationException($"{fieldName} must be a finite number.");
        }
    }

    private static bool IsUniqueNameViolation(DbUpdateException exception) =>
        exception.InnerException?.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase) == true;

    private static EquipmentResponse Map(Equipment equipment, ReadingSource lastReadingSource) =>
        new(
            equipment.Id,
            equipment.Name,
            equipment.MinimumValue,
            equipment.MaximumValue,
            equipment.CurrentValue,
            lastReadingSource,
            equipment.CreatedAt,
            equipment.UpdatedAt);
}
