using Reservation.NotificationsService.DAL.Enums;

namespace Reservation.NotificationsService.DAL.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public DateTime SentAt { get; set; }
        public bool Read { get; set; }
    }
}
