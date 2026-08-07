using DevicePulse.Domain.Enums;

namespace DevicePulse.Domain.Entities;

public sealed class SimulationProfile
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid OrganizationId { get; init; }
    public required Guid DeviceId { get; init; }
    public required string Name { get; set; }
    public SimulationMode Mode { get; set; }
    public int IntervalMilliseconds { get; set; } = 1000;
    public double MinimumValue { get; set; }
    public double MaximumValue { get; set; }
    public double Step { get; set; } = 1;
    public double BaseValue { get; set; }
    public double NoiseAmplitude { get; set; } = 1;
    public double FailureProbability { get; set; } = .1;
    public bool ReverseAtBoundary { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public sealed class SimulationSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid SimulationProfileId { get; init; }
    public required Guid DeviceId { get; init; }
    public SimulationStatus Status { get; private set; } = SimulationStatus.Running;
    public DateTime StartedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime? PausedAtUtc { get; private set; }
    public DateTime? StoppedAtUtc { get; private set; }
    public DateTime? LastGeneratedAtUtc { get; set; }
    public void Pause(DateTime now) { if (Status != SimulationStatus.Running) throw new InvalidOperationException(); Status = SimulationStatus.Paused; PausedAtUtc = now; }
    public void Resume() { if (Status != SimulationStatus.Paused) throw new InvalidOperationException(); Status = SimulationStatus.Running; PausedAtUtc = null; }
    public void Stop(DateTime now) { Status = SimulationStatus.Stopped; StoppedAtUtc = now; }
}
