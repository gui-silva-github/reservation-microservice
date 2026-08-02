using Reservation.NotificationsService.DAL.Entities;

namespace Reservation.NotificationsService.DAL.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification> CreateAsync(Notification notification);
        Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId);
    }
}
