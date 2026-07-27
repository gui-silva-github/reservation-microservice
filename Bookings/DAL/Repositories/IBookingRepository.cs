using Reservation.BookingsService.DAL.Entities;
using Reservation.BookingsService.DAL.Enums;

namespace Reservation.BookingsService.DAL.Repositories
{
    public interface IBookingRepository
    {
        Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken = default);
        
        Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Booking>> GetBySpaceIdAsync(Guid spaceId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Booking>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);

        Task<bool> HasConflictBookingAsync(
            Guid spaceId,
            DateTime startDate,
            DateTime endDate,
            Guid? excludeBookingId = null,
            CancellationToken cancellationToken = default
        );

        Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken = default);

        Task<Booking?> UpdateAsync(Booking booking, CancellationToken cancellationToken = default);

        Task<bool> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    }
}