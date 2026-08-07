using DevicePulse.Api.Models;

namespace DevicePulse.Api.Contracts;

public sealed record EquipmentResponse(
    long Id,
    string Name,
    double MinimumValue,
    double MaximumValue,
    double CurrentValue,
    ReadingSource LastReadingSource,
    DateTime CreatedAt,
    DateTime UpdatedAt);
