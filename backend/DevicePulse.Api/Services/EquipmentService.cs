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

    public async Task<EquipmentResponse?> UpdateAsync(
        long id,
        UpdateEquipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = ValidateAndNormalize(
            request.Name,
            request.MinimumValue,
            request.MaximumValue);
        var equipment = await dbContext.Equipments
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (equipment is null)
        {
            return null;
        }

        if (await dbContext.Equipments.AnyAsync(
                item => item.Id != id && item.Name == normalizedName,
                cancellationToken))
        {
            throw new DuplicateEquipmentNameException(normalizedName);
        }

        equipment.Name = normalizedName;
        equipment.MinimumValue = request.MinimumValue;
        equipment.MaximumValue = request.MaximumValue;
        equipment.UpdatedAt = DateTime.UtcNow;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueNameViolation(exception))
        {
            throw new DuplicateEquipmentNameException(normalizedName);
        }

        var lastReadingSource = await dbContext.EquipmentReadings
            .Where(reading => reading.EquipmentId == equipment.Id)
            .OrderByDescending(reading => reading.RecordedAt)
            .ThenByDescending(reading => reading.Id)
            .Select(reading => reading.Source)
            .FirstAsync(cancellationToken);

        return Map(equipment, lastReadingSource);
    }

    public async Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var equipment = await dbContext.Equipments.SingleOrDefaultAsync(
            item => item.Id == id,
            cancellationToken);

        if (equipment is null)
        {
            return false;
        }

        dbContext.Equipments.Remove(equipment);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<EquipmentReadingResponse?> CreateReadingAsync(
        long id,
        CreateReadingRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateFinite(request.Value, nameof(request.Value));

        if (request.Source is not ReadingSource.Manual and not ReadingSource.Autopilot)
        {
            throw new DomainValidationException("Source must be Manual or Autopilot.");
        }

        var equipment = await dbContext.Equipments.SingleOrDefaultAsync(
            item => item.Id == id,
            cancellationToken);

        if (equipment is null)
        {
            return null;
        }

        var reading = new EquipmentReading
        {
            EquipmentId = equipment.Id,
            Value = request.Value,
            Source = request.Source,
            RecordedAt = DateTime.UtcNow
        };

        equipment.CurrentValue = reading.Value;
        equipment.UpdatedAt = reading.RecordedAt;
        dbContext.EquipmentReadings.Add(reading);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new EquipmentReadingResponse(
            reading.Id,
            reading.EquipmentId,
            reading.Value,
            reading.Source,
            reading.RecordedAt);
    }

    public async Task<IReadOnlyList<EquipmentReadingResponse>?> GetReadingsAsync(
        long id,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > 100)
        {
            throw new DomainValidationException("Limit must be between 1 and 100.");
        }

        if (!await dbContext.Equipments.AsNoTracking().AnyAsync(
                equipment => equipment.Id == id,
                cancellationToken))
        {
            return null;
        }

        return await dbContext.EquipmentReadings
            .AsNoTracking()
            .Where(reading => reading.EquipmentId == id)
            .OrderByDescending(reading => reading.RecordedAt)
            .ThenByDescending(reading => reading.Id)
            .Take(limit)
            .Select(reading => new EquipmentReadingResponse(
                reading.Id,
                reading.EquipmentId,
                reading.Value,
                reading.Source,
                reading.RecordedAt))
            .ToListAsync(cancellationToken);
    }

    private static string ValidateAndNormalize(CreateEquipmentRequest request)
    {
        var name = ValidateAndNormalize(request.Name, request.MinimumValue, request.MaximumValue);

        ValidateFinite(request.CurrentValue, nameof(request.CurrentValue));

        return name;
    }

    private static string ValidateAndNormalize(
        string? requestedName,
        double minimumValue,
        double maximumValue)
    {
        var name = requestedName?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Name is required.");
        }

        if (name.Length > 100)
        {
            throw new DomainValidationException("Name must not exceed 100 characters.");
        }

        ValidateFinite(minimumValue, "MinimumValue");
        ValidateFinite(maximumValue, "MaximumValue");

        if (minimumValue >= maximumValue)
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
