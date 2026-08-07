using DevicePulse.Domain.Entities;
using DevicePulse.Domain.Enums;

namespace DevicePulse.Domain.Services;

public sealed class SimulationValueGenerator(Random? random = null)
{
    private readonly Random random = random ?? Random.Shared;
    public double? Next(SimulationProfile profile, double previous, long iteration)
    {
        if (!profile.IsEnabled || profile.MinimumValue >= profile.MaximumValue || profile.IntervalMilliseconds <= 0) throw new ArgumentException("Invalid simulation profile.");
        var span = profile.MaximumValue - profile.MinimumValue;
        return profile.Mode switch
        {
            SimulationMode.Random => profile.MinimumValue + random.NextDouble() * span,
            SimulationMode.LinearIncrease => previous + profile.Step > profile.MaximumValue ? profile.MinimumValue : previous + profile.Step,
            SimulationMode.LinearDecrease => previous - profile.Step < profile.MinimumValue ? profile.MaximumValue : previous - profile.Step,
            SimulationMode.SineWave => profile.MinimumValue + span * (Math.Sin(iteration * Math.PI / 10) + 1) / 2,
            SimulationMode.StableWithNoise => Math.Clamp(profile.BaseValue + (random.NextDouble() * 2 - 1) * profile.NoiseAmplitude, profile.MinimumValue, profile.MaximumValue),
            SimulationMode.FailureSimulation => random.NextDouble() < profile.FailureProbability ? null : profile.MinimumValue + random.NextDouble() * span,
            _ => throw new ArgumentOutOfRangeException(nameof(profile.Mode))
        };
    }
}
