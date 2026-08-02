using Dapper;
using Reservation.NotificationsService.DAL.Database;
using Reservation.NotificationsService.DAL.Entities;

namespace Reservation.NotificationsService.DAL.Repositories
{
    internal sealed class NotificationRepository : INotificationRepository
    {
        private const string SelectCollumns = """
            id AS Id,
            user_id AS UserId,
            message AS Message,
            type AS Type,
            sent_at AS SentAt,
            read AS Read
            """;

        private readonly DapperDbContext _dbContext;

        public NotificationRepository(DapperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            const string query = """
                INSERT INTO notifications (id, user_id, message, type, sent_at, read)
                VALUES (@Id, @UserId, @Message, @Type, @SentAt, @Read)
                """;

            await _dbContext.Connection.ExecuteAsync(query, new
            {
                notification.Id,
                notification.UserId,
                notification.Message,
                Type = notification.Type.ToString(),
                notification.SentAt,
                notification.Read
            });

            return notification;
        }

        public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId)
        {
            string query = $"""
                SELECT {SelectCollumns}
                FROM notifications
                WHERE user_id = @UserId
                ORDER BY sent_at DESC
                """;

            IEnumerable<Notification> notifications = await _dbContext.Connection.QueryAsync<Notification>(
                query,
                new { UserId = userId });

            return notifications.ToList();
        }
    }
}
