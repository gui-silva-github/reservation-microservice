using Reservation.SpacesService.BLL.DTO;

namespace Reservation.SpacesService.BLL.Services
{
    public interface ISpaceService
    {
        Task<IReadOnlyList<SpaceResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<SpaceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SpaceResponse>> SearchAsync(string term, CancellationToken cancellationToken = default);
        Task<AvailabilityResponse> CheckAvailabilityAsync(
            Guid id,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default
        );
        Task<SpaceResponse> CreateAsync(CreateSpaceRequest request, CancellationToken cancellationToken = default);
        Task<SpaceResponse> UpdateAsync(Guid id, UpdateSpaceRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}