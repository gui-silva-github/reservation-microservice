using Reservation.NotificationsService.BLL.DTOs;

namespace Reservation.NotificationsService.BLL.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponse> SendAsync(SendNotificationRequest request);
        Task<IReadOnlyList<NotificationResponse>> GetByUserIdAsync(Guid userId, Guid? authenticatedUserId, string? role);
        Task<NotificationResponse> MarkAsReadAsync(Guid id, Guid? authenticatedUserId, string? role);
        Task MarkAllAsReadAsync(Guid userId, Guid? authenticatedUserId, string? role);
        Task ProcessBookingCreatedEventAsync(BookingCreatedEvent bookingEvent);
    }
}
