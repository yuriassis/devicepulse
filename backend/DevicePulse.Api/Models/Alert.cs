namespace DevicePulse.Api.Models;

public sealed class Alert
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public long EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;
    public double MinimumValue { get; set; }
    public double MaximumValue { get; set; }
    public DateTime CreatedAt { get; set; }
}
