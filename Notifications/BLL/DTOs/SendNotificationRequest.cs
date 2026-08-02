using Reservation.NotificationsService.DAL.Enums;

namespace Reservation.NotificationsService.BLL.DTOs
{
    public record SendNotificationRequest(
        Guid UserId,
        string Message,
        NotificationType Type);
}
