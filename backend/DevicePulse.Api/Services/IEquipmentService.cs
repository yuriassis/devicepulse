using DevicePulse.Api.Contracts;

namespace DevicePulse.Api.Services;

public interface IEquipmentService
{
    Task<EquipmentResponse> CreateAsync(
        CreateEquipmentRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EquipmentResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<EquipmentResponse?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);
}
