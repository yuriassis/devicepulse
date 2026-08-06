namespace DevicePulse.Api.Contracts;

public sealed record CreateEquipmentRequest(
    string? Name,
    double MinimumValue,
    double MaximumValue,
    double CurrentValue);
