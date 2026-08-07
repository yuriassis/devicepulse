using DevicePulse.Domain.Enums;

namespace DevicePulse.Domain.Entities;

public sealed class Device
{
    public const int MaximumNameLength = 100;
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid OrganizationId { get; init; }
    public Guid? AreaId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Unit { get; set; }
    public double MinimumValue { get; set; }
    public double MaximumValue { get; set; }
    public double CurrentValue { get; private set; }
    public DateTime? LastReadingAtUtc { get; private set; }
    public int OfflineTimeoutSeconds { get; set; } = 300;
    public DeviceStatus Status { get; private set; } = DeviceStatus.Unknown;
    public bool IsEnabled { get; private set; } = true;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Trim().Length > MaximumNameLength) throw new ArgumentException("A device name between 1 and 100 characters is required.");
        if (!double.IsFinite(MinimumValue) || !double.IsFinite(MaximumValue) || MinimumValue >= MaximumValue) throw new ArgumentException("MinimumValue must be less than MaximumValue.");
        if (OfflineTimeoutSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(OfflineTimeoutSeconds));
    }

    public void Record(double value, DateTime timestampUtc)
    {
        if (!IsEnabled) throw new InvalidOperationException("A disabled device cannot receive telemetry.");
        if (!double.IsFinite(value) || timestampUtc.Kind != DateTimeKind.Utc) throw new ArgumentException("Telemetry must be finite and use UTC.");
        CurrentValue = value; LastReadingAtUtc = timestampUtc; Status = DeviceStatus.Online; UpdatedAtUtc = DateTime.UtcNow;
    }

    public bool MarkOffline(DateTime nowUtc)
    {
        if (!IsEnabled || LastReadingAtUtc is null || nowUtc - LastReadingAtUtc <= TimeSpan.FromSeconds(OfflineTimeoutSeconds)) return false;
        if (Status == DeviceStatus.Offline) return false;
        Status = DeviceStatus.Offline; UpdatedAtUtc = nowUtc; return true;
    }

    public void SetAlertStatus(bool warning, bool critical) => Status = critical ? DeviceStatus.Critical : warning ? DeviceStatus.Warning : DeviceStatus.Online;
    public void Disable() { IsEnabled = false; Status = DeviceStatus.Disabled; UpdatedAtUtc = DateTime.UtcNow; }
}
