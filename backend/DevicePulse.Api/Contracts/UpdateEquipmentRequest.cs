namespace DevicePulse.Api.Contracts;

public sealed record UpdateEquipmentRequest(
    string? Name,
    double MinimumValue,
    double MaximumValue);
