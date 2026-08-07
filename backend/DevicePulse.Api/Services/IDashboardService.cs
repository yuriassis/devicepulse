using DevicePulse.Api.Contracts;

namespace DevicePulse.Api.Services;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default);
}
