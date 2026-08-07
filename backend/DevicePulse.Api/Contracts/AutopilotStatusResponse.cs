namespace DevicePulse.Api.Contracts;

public sealed record AutopilotStatusResponse(
    bool IsRunning,
    int IntervalSeconds,
    DateTime? LastRunAt);
