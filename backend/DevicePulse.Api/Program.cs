using DevicePulse.Api.Data;
using DevicePulse.Api.Middleware;
using DevicePulse.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DevicePulseDbContext>(options =>
{
    var connection = builder.Configuration.GetConnectionString("DevicePulse");
    if (builder.Environment.IsEnvironment("Testing")) options.UseSqlite(connection);
    else options.UseNpgsql(connection, npgsql => npgsql.EnableRetryOnFailure());
});
builder.Services.AddSignalR();
builder.Services.AddHealthChecks().AddDbContextCheck<DevicePulseDbContext>();
builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAutopilotReadingGenerator, AutopilotReadingGenerator>();
builder.Services.AddSingleton<AutopilotService>();
builder.Services.AddSingleton<IAutopilotService>(provider => provider.GetRequiredService<AutopilotService>());
builder.Services.AddHostedService(provider => provider.GetRequiredService<AutopilotService>());

var app = builder.Build();

app.Use(async (context, next) =>
{
    const string header = "X-Correlation-ID";
    var correlationId = context.Request.Headers[header].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
    context.Response.Headers[header] = correlationId;
    using (app.Logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId })) await next();
});
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DevicePulseDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHub<DevicePulse.Api.Realtime.DeviceUpdatesHub>("/hubs/device-updates");
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
