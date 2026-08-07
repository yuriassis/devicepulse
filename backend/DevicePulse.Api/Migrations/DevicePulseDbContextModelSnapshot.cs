using DevicePulse.Api.Data;
using DevicePulse.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace DevicePulse.Api.Migrations;

[DbContext(typeof(DevicePulseDbContext))]
partial class DevicePulseDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder) => ApplyModel(modelBuilder);

    internal static void ApplyModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.22");

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.Property(item => item.Id).ValueGeneratedOnAdd().HasColumnType("INTEGER");
            entity.Property(item => item.CreatedAt).HasColumnType("TEXT");
            entity.Property(item => item.EquipmentId).HasColumnType("INTEGER");
            entity.Property(item => item.MaximumValue).HasColumnType("REAL");
            entity.Property(item => item.MinimumValue).HasColumnType("REAL");
            entity.Property(item => item.Name).IsRequired().HasMaxLength(100).HasColumnType("TEXT");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.EquipmentId);
            entity.ToTable("Alerts");
            entity.HasOne(item => item.Equipment)
                .WithMany(item => item.Alerts)
                .HasForeignKey(item => item.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.Property(item => item.Id).ValueGeneratedOnAdd().HasColumnType("INTEGER");
            entity.Property(item => item.CreatedAt).HasColumnType("TEXT");
            entity.Property(item => item.CurrentValue).HasColumnType("REAL");
            entity.Property(item => item.MaximumValue).HasColumnType("REAL");
            entity.Property(item => item.MinimumValue).HasColumnType("REAL");
            entity.Property(item => item.Name).IsRequired().HasMaxLength(100).UseCollation("NOCASE").HasColumnType("TEXT");
            entity.Property(item => item.UpdatedAt).HasColumnType("TEXT");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.Name).IsUnique();
            entity.ToTable("Equipments");
        });

        modelBuilder.Entity<EquipmentReading>(entity =>
        {
            entity.Property(item => item.Id).ValueGeneratedOnAdd().HasColumnType("INTEGER");
            entity.Property(item => item.EquipmentId).HasColumnType("INTEGER");
            entity.Property(item => item.RecordedAt).HasColumnType("TEXT");
            entity.Property(item => item.Source).HasMaxLength(20).HasConversion<string>().HasColumnType("TEXT");
            entity.Property(item => item.Value).HasColumnType("REAL");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new { item.EquipmentId, item.RecordedAt });
            entity.ToTable("EquipmentReadings");
            entity.HasOne(item => item.Equipment)
                .WithMany(item => item.Readings)
                .HasForeignKey(item => item.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
    }
}
