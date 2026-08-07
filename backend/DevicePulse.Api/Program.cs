using DevicePulse.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<DevicePulseDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DevicePulse")));

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program;
