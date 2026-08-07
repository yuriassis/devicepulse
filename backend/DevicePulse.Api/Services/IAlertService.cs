using DevicePulse.Api.Contracts;

namespace DevicePulse.Api.Services;

public interface IAlertService
{
    Task<AlertResponse> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlertResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
