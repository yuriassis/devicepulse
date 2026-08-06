using DevicePulse.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevicePulse.Api.Migrations;

[DbContext(typeof(DevicePulseDbContext))]
[Migration("20260806000000_InitialCreate")]
partial class InitialCreate
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        DevicePulseDbContextModelSnapshot.ApplyModel(modelBuilder);
    }
}
