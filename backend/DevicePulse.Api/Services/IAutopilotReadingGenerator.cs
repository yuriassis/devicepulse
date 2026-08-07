namespace DevicePulse.Api.Services;

public interface IAutopilotReadingGenerator
{
    Task<int> GenerateAsync(CancellationToken cancellationToken = default);
}
