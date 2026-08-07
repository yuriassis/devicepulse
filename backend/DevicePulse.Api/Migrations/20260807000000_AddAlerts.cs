using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DevicePulse.Api.Migrations;

public partial class AddAlerts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var postgres = migrationBuilder.ActiveProvider.Contains("Npgsql", StringComparison.Ordinal);
        var integer = postgres ? "bigint" : "INTEGER";
        var number = postgres ? "double precision" : "REAL";
        var timestamp = postgres ? "timestamp with time zone" : "TEXT";
        migrationBuilder.CreateTable(
            name: "Alerts",
            columns: table => new
            {
                Id = table.Column<long>(type: integer, nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                EquipmentId = table.Column<long>(type: integer, nullable: false),
                MinimumValue = table.Column<double>(type: number, nullable: false),
                MaximumValue = table.Column<double>(type: number, nullable: false),
                CreatedAt = table.Column<DateTime>(type: timestamp, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Alerts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Alerts_Equipments_EquipmentId",
                    column: x => x.EquipmentId,
                    principalTable: "Equipments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_Alerts_EquipmentId", table: "Alerts", column: "EquipmentId");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "Alerts");
}
