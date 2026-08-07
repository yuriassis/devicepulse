using DevicePulse.Domain.Enums;

namespace DevicePulse.Domain.Entities;

public sealed class AlertRule
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid OrganizationId { get; init; }
    public required Guid DeviceId { get; init; }
    public required string Name { get; set; }
    public AlertOperator Operator { get; set; }
    public double Threshold { get; set; }
    public double? SecondaryThreshold { get; set; }
    public AlertSeverity Severity { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsViolated(double value)
    {
        if (!IsEnabled) return false;
        if (!double.IsFinite(Threshold) || !double.IsFinite(value)) throw new ArgumentException("Alert values must be finite.");
        var range = Operator is AlertOperator.InsideRange or AlertOperator.OutsideRange;
        if (range && (SecondaryThreshold is null || Threshold >= SecondaryThreshold || !double.IsFinite(SecondaryThreshold.Value))) throw new InvalidOperationException("Range rules require ordered finite thresholds.");
        return Operator switch
        {
            AlertOperator.GreaterThan => value > Threshold,
            AlertOperator.GreaterThanOrEqual => value >= Threshold,
            AlertOperator.LessThan => value < Threshold,
            AlertOperator.LessThanOrEqual => value <= Threshold,
            AlertOperator.Equal => value.Equals(Threshold),
            AlertOperator.OutsideRange => value < Threshold || value > SecondaryThreshold,
            AlertOperator.InsideRange => value >= Threshold && value <= SecondaryThreshold,
            _ => false
        };
    }
}
