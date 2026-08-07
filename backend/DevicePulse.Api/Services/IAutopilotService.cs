using DevicePulse.Api.Contracts;

namespace DevicePulse.Api.Services;

public interface IAutopilotService
{
    AutopilotStatusResponse GetStatus();

    AutopilotStatusResponse Start();

    AutopilotStatusResponse Stop();
}
