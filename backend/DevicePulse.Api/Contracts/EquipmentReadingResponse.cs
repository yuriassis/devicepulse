using DevicePulse.Api.Models;

namespace DevicePulse.Api.Contracts;

public sealed record EquipmentReadingResponse(
    long Id,
    long EquipmentId,
    double Value,
    ReadingSource Source,
    DateTime RecordedAt);
