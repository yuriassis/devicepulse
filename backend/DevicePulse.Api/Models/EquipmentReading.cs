namespace DevicePulse.Api.Models;

public sealed class EquipmentReading
{
    public long Id { get; set; }

    public long EquipmentId { get; set; }

    public Equipment Equipment { get; set; } = null!;

    public double Value { get; set; }

    public ReadingSource Source { get; set; }

    public DateTime RecordedAt { get; set; }
}
