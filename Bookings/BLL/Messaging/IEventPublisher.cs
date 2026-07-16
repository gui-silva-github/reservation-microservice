using Reservation.BookingsService.BLL.DTO;

namespace Reservation.BookingsService.BLL.Messaging
{
    public interface IEventPublisher
    {
        Task PublishBookingCreatedAsync(BookingCreatedEvent bookingCreatedEvent, CancellationToken cancellationToken = default);
    }
}