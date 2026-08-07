using DevicePulse.Api.Contracts;

namespace DevicePulse.Api.Services;

public sealed class AutopilotService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<AutopilotService> logger) : BackgroundService, IAutopilotService
{
    private readonly int intervalSeconds = Math.Max(1, configuration.GetValue("Autopilot:IntervalSeconds", 10));
    private int isRunning;
    private long lastRunTicks;

    public AutopilotStatusResponse GetStatus() => CreateStatus();

    public AutopilotStatusResponse Start()
    {
        Interlocked.Exchange(ref isRunning, 1);
        return CreateStatus();
    }

    public AutopilotStatusResponse Stop()
    {
        Interlocked.Exchange(ref isRunning, 0);
        return CreateStatus();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (Volatile.Read(ref isRunning) == 0)
            {
                continue;
            }

            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var generator = scope.ServiceProvider.GetRequiredService<IAutopilotReadingGenerator>();
                var count = await generator.GenerateAsync(stoppingToken);
                Interlocked.Exchange(ref lastRunTicks, DateTime.UtcNow.Ticks);
                logger.LogInformation("Autopilot generated readings for {EquipmentCount} equipment(s).", count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Autopilot reading generation failed.");
            }
        }
    }

    private AutopilotStatusResponse CreateStatus()
    {
        var ticks = Interlocked.Read(ref lastRunTicks);
        return new AutopilotStatusResponse(
            Volatile.Read(ref isRunning) == 1,
            intervalSeconds,
            ticks == 0 ? null : new DateTime(ticks, DateTimeKind.Utc));
    }
}
