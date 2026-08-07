using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DevicePulse.Api.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var postgres = migrationBuilder.ActiveProvider.Contains("Npgsql", StringComparison.Ordinal);
        var integer = postgres ? "bigint" : "INTEGER";
        var number = postgres ? "double precision" : "REAL";
        var timestamp = postgres ? "timestamp with time zone" : "TEXT";
        migrationBuilder.CreateTable(
            name: "Equipments",
            columns: table => new
            {
                Id = table.Column<long>(type: integer, nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                MinimumValue = table.Column<double>(type: number, nullable: false),
                MaximumValue = table.Column<double>(type: number, nullable: false),
                CurrentValue = table.Column<double>(type: number, nullable: false),
                CreatedAt = table.Column<DateTime>(type: timestamp, nullable: false),
                UpdatedAt = table.Column<DateTime>(type: timestamp, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Equipments", x => x.Id));

        migrationBuilder.CreateTable(
            name: "EquipmentReadings",
            columns: table => new
            {
                Id = table.Column<long>(type: integer, nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                EquipmentId = table.Column<long>(type: integer, nullable: false),
                Value = table.Column<double>(type: number, nullable: false),
                Source = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                RecordedAt = table.Column<DateTime>(type: timestamp, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EquipmentReadings", x => x.Id);
                table.ForeignKey(
                    name: "FK_EquipmentReadings_Equipments_EquipmentId",
                    column: x => x.EquipmentId,
                    principalTable: "Equipments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_EquipmentReadings_EquipmentId_RecordedAt",
            table: "EquipmentReadings",
            columns: new[] { "EquipmentId", "RecordedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Equipments_Name",
            table: "Equipments",
            column: "Name",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "EquipmentReadings");
        migrationBuilder.DropTable(name: "Equipments");
    }
}
