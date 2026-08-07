using DevicePulse.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DevicePulse.Api.Data;

public sealed class DevicePulseDbContext(DbContextOptions<DevicePulseDbContext> options)
    : DbContext(options)
{
    public DbSet<Equipment> Equipments => Set<Equipment>();

    public DbSet<EquipmentReading> EquipmentReadings => Set<EquipmentReading>();

    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var equipment = modelBuilder.Entity<Equipment>();
        equipment.ToTable("Equipments");
        equipment.HasKey(item => item.Id);
        equipment.Property(item => item.Name).IsRequired().HasMaxLength(100).UseCollation("NOCASE");
        equipment.HasIndex(item => item.Name).IsUnique();
        equipment.HasMany(item => item.Readings)
            .WithOne(item => item.Equipment)
            .HasForeignKey(item => item.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        var reading = modelBuilder.Entity<EquipmentReading>();
        reading.ToTable("EquipmentReadings");
        reading.HasKey(item => item.Id);
        reading.Property(item => item.Source).HasConversion<string>().HasMaxLength(20);
        reading.HasIndex(item => new { item.EquipmentId, item.RecordedAt });

        var alert = modelBuilder.Entity<Alert>();
        alert.ToTable("Alerts");
        alert.HasKey(item => item.Id);
        alert.Property(item => item.Name).IsRequired().HasMaxLength(100);
        alert.HasOne(item => item.Equipment)
            .WithMany(item => item.Alerts)
            .HasForeignKey(item => item.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);
        alert.HasIndex(item => item.EquipmentId);
    }
}
