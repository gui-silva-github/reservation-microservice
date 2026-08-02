using Reservation.NotificationsService.BLL.DTOs;

namespace Reservation.NotificationsService.BLL.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponse> SendAsync(SendNotificationRquest request);
        Task<IReadOnlyList<NotificationResponse>> GetByUserIdAsync(Guid userId, Guid? authenticatedUserId, string? role);
        Task ProcessBookingCreatedEventAsync(BookingCreatedEvent bookingEvent);
    }
}
