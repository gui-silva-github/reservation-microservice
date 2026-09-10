using Reservation.NotificationsService.DAL.Entities;

namespace Reservation.NotificationsService.DAL.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification> CreateAsync(Notification notification);
        Task<Notification?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid id);
        Task<int> MarkAllAsReadByUserIdAsync(Guid userId);
    }
}
