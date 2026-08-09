using Reservation.SpacesService.DAL.Entities;

namespace Reservation.SpacesService.DAL.Repositories
{
    public interface ISpaceRepository
    {
        Task<IReadOnlyList<Space>> GetAllActiveAsync(CancellationToken cancellationToken = default);
        Task<Space?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Space>> SearchActiveAsync(string term, CancellationToken cancellationToken = default);
        Task<Space> AddAsync(Space space, CancellationToken cancellationToken = default);
        Task<Space?> UpdateAsync(Space space, CancellationToken cancellationToken = default);
        Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}