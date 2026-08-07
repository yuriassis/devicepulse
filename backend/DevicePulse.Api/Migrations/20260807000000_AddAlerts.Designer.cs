using DevicePulse.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevicePulse.Api.Migrations;

[DbContext(typeof(DevicePulseDbContext))]
[Migration("20260807000000_AddAlerts")]
partial class AddAlerts
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        DevicePulseDbContextModelSnapshot.ApplyModel(modelBuilder);
}
