using Reservation.SpacesService.BLL.DTO;

namespace Reservation.SpacesService.BLL.HttpClients
{
    public interface IBookingsHttpClient
    {
        Task<IReadOnlyList<BookingResponse>> GetBookingsBySpaceIdAsync(
            Guid spaceId,
            CancellationToken cancellationToken = default
        );
    }
}