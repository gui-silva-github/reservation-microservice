using Reservation.NotificationsService.DAL.Enums;

namespace Reservation.NotificationsService.BLL.DTOs
{
    public record NotificationResponse(
        Guid Id,
        Guid UserId,
        string Message,
        NotificationType Type,
        DateTime SentAt,
        bool Read);
}
