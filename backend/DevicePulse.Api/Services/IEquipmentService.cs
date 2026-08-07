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

    Task<EquipmentResponse?> UpdateAsync(
        long id,
        UpdateEquipmentRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<EquipmentReadingResponse?> CreateReadingAsync(
        long id,
        CreateReadingRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EquipmentReadingResponse>?> GetReadingsAsync(
        long id,
        int limit,
        CancellationToken cancellationToken = default);
}
