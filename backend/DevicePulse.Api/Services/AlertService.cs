using DevicePulse.Api.Contracts;
using DevicePulse.Api.Data;
using DevicePulse.Api.Exceptions;
using DevicePulse.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DevicePulse.Api.Services;

public sealed class AlertService(DevicePulseDbContext dbContext) : IAlertService
{
    public async Task<AlertResponse> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Name is required.");
        if (name.Length > 100)
            throw new DomainValidationException("Name must contain at most 100 characters.");
        if (!double.IsFinite(request.MinimumValue) || !double.IsFinite(request.MaximumValue))
            throw new DomainValidationException("Alert limits must be finite numbers.");
        if (request.MinimumValue >= request.MaximumValue)
            throw new DomainValidationException("MinimumValue must be less than MaximumValue.");

        var equipment = await dbContext.Equipments.SingleOrDefaultAsync(
            item => item.Id == request.EquipmentId, cancellationToken)
            ?? throw new DomainValidationException("Equipment does not exist.");
        var alert = new Alert
        {
            Name = name,
            EquipmentId = equipment.Id,
            Equipment = equipment,
            MinimumValue = request.MinimumValue,
            MaximumValue = request.MaximumValue,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Alerts.Add(alert);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(alert, equipment);
    }

    public async Task<IReadOnlyList<AlertResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Alerts.AsNoTracking()
            .OrderBy(alert => alert.Name)
            .Select(alert => new AlertResponse(
                alert.Id, alert.Name, alert.EquipmentId, alert.Equipment.Name,
                alert.MinimumValue, alert.MaximumValue, alert.Equipment.CurrentValue,
                alert.Equipment.CurrentValue < alert.MinimumValue || alert.Equipment.CurrentValue > alert.MaximumValue,
                alert.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var alert = await dbContext.Alerts.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (alert is null) return false;
        dbContext.Alerts.Remove(alert);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static AlertResponse Map(Alert alert, Equipment equipment) => new(
        alert.Id, alert.Name, equipment.Id, equipment.Name, alert.MinimumValue,
        alert.MaximumValue, equipment.CurrentValue,
        equipment.CurrentValue < alert.MinimumValue || equipment.CurrentValue > alert.MaximumValue,
        alert.CreatedAt);
}
