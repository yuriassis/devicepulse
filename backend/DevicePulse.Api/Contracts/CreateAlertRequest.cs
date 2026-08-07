namespace DevicePulse.Api.Contracts;

public sealed record CreateAlertRequest(
    string? Name,
    long EquipmentId,
    double MinimumValue,
    double MaximumValue);
