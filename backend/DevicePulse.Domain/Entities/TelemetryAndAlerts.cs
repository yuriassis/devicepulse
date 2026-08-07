using DevicePulse.Domain.Enums;

namespace DevicePulse.Domain.Entities;

public sealed record TelemetryReading(Guid Id, Guid OrganizationId, Guid DeviceId, double Value, DateTime TimestampUtc, DateTime ReceivedAtUtc, TelemetrySource Source, Guid MessageId);

public sealed class AlertOccurrence
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid OrganizationId { get; init; }
    public required Guid DeviceId { get; init; }
    public required Guid AlertRuleId { get; init; }
    public AlertStatus Status { get; private set; } = AlertStatus.Active;
    public required AlertSeverity Severity { get; init; }
    public required double TriggeredValue { get; init; }
    public required DateTime TriggeredAtUtc { get; init; }
    public double LastObservedValue { get; private set; }
    public DateTime LastObservedAtUtc { get; private set; }
    public DateTime? AcknowledgedAtUtc { get; private set; }
    public string? AcknowledgedByUserId { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }

    public void Observe(double value, DateTime atUtc) { LastObservedValue = value; LastObservedAtUtc = atUtc; }
    public void Acknowledge(string userId, DateTime atUtc) { if (Status == AlertStatus.Resolved) throw new InvalidOperationException("Resolved alerts cannot be acknowledged."); Status = AlertStatus.Acknowledged; AcknowledgedByUserId = userId; AcknowledgedAtUtc = atUtc; }
    public void Resolve(DateTime atUtc) { Status = AlertStatus.Resolved; ResolvedAtUtc = atUtc; }
}
