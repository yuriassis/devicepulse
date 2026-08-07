using DevicePulse.Domain.Entities;
using DevicePulse.Domain.Enums;
using DevicePulse.Domain.Services;

namespace DevicePulse.Tests.Domain;

public sealed class DomainModelTests
{
    [Theory]
    [InlineData(AlertOperator.GreaterThan, 10d, null, 11d, true)]
    [InlineData(AlertOperator.GreaterThanOrEqual, 10d, null, 10d, true)]
    [InlineData(AlertOperator.LessThan, 10d, null, 9d, true)]
    [InlineData(AlertOperator.LessThanOrEqual, 10d, null, 10d, true)]
    [InlineData(AlertOperator.Equal, 10d, null, 10d, true)]
    [InlineData(AlertOperator.OutsideRange, 10d, 20d, 21d, true)]
    [InlineData(AlertOperator.InsideRange, 10d, 20d, 15d, true)]
    public void Alert_rule_evaluates_every_operator(AlertOperator op, double threshold, double? secondary, double value, bool expected)
    {
        var rule = new AlertRule { OrganizationId = Guid.NewGuid(), DeviceId = Guid.NewGuid(), Name = "rule", Operator = op, Threshold = threshold, SecondaryThreshold = secondary };
        Assert.Equal(expected, rule.IsViolated(value));
    }

    [Fact]
    public void Device_only_transitions_offline_after_timeout()
    {
        var now = new DateTime(2026, 8, 7, 10, 0, 0, DateTimeKind.Utc);
        var device = NewDevice();
        device.Record(42, now);
        Assert.False(device.MarkOffline(now.AddSeconds(30)));
        Assert.True(device.MarkOffline(now.AddSeconds(61)));
        Assert.Equal(DeviceStatus.Offline, device.Status);
    }

    [Fact]
    public void Acknowledgement_does_not_resolve_an_alert()
    {
        var occurrence = new AlertOccurrence { OrganizationId = Guid.NewGuid(), DeviceId = Guid.NewGuid(), AlertRuleId = Guid.NewGuid(), Severity = AlertSeverity.Warning, TriggeredValue = 99, TriggeredAtUtc = DateTime.UtcNow };
        occurrence.Acknowledge("operator", DateTime.UtcNow);
        Assert.Equal(AlertStatus.Acknowledged, occurrence.Status);
        Assert.Null(occurrence.ResolvedAtUtc);
    }

    [Theory]
    [InlineData(SimulationMode.Random)]
    [InlineData(SimulationMode.LinearIncrease)]
    [InlineData(SimulationMode.LinearDecrease)]
    [InlineData(SimulationMode.SineWave)]
    [InlineData(SimulationMode.StableWithNoise)]
    public void Simulation_modes_generate_bounded_values(SimulationMode mode)
    {
        var profile = new SimulationProfile { OrganizationId = Guid.NewGuid(), DeviceId = Guid.NewGuid(), Name = "profile", Mode = mode, MinimumValue = 0, MaximumValue = 100, BaseValue = 50 };
        var value = new SimulationValueGenerator(new Random(42)).Next(profile, 50, 1);
        Assert.InRange(value!.Value, 0, 100);
    }

    private static Device NewDevice() => new() { OrganizationId = Guid.NewGuid(), Name = "device", MinimumValue = 0, MaximumValue = 100, OfflineTimeoutSeconds = 60 };
}
