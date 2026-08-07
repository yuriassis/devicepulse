namespace DevicePulse.Api.Models;

public sealed class Equipment
{
    public long Id { get; set; }

    public required string Name { get; set; }

    public double MinimumValue { get; set; }

    public double MaximumValue { get; set; }

    public double CurrentValue { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<EquipmentReading> Readings { get; set; } = [];

    public ICollection<Alert> Alerts { get; set; } = [];
}
