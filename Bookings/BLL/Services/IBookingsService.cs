using Reservation.BookingsServices.BLL.DTO;

namespace Reservation.BookingsService.BLL.Services
{
    public interface IBookingsService
    {
        Task<IReadOnlyList<BookingResponse>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<BookingResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<BookingResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<BookingResponse>> GetBySpaceIdAsync(Guid spaceId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<BookingResponse>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);

        Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);

        Task<BookingResponse> UpdateAsync(Guid id, UpdateBookingRequest request, CancellationToken cancellationToken = default);

        Task CancelAsync(Guid id, CancellationToken cancellationToken = default); 
    }
}