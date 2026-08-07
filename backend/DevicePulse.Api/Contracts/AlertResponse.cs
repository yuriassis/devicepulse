namespace DevicePulse.Api.Contracts;

public sealed record AlertResponse(
    long Id,
    string Name,
    long EquipmentId,
    string EquipmentName,
    double MinimumValue,
    double MaximumValue,
    double CurrentValue,
    bool IsTriggered,
    DateTime CreatedAt);
