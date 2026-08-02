using Dapper;
using Microsoft.Extensions.Logging;

namespace Reservation.NotificationsService.DAL.Database
{
    public sealed class DatabaseInitializer
    {
        private readonly DapperDbContext _dbContext;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(DapperDbContext dbContext, ILogger<DatabaseInitializer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            const string createTableSql = """
                CREATE TABLE IF NOT EXISTS notifications (
                    id UUID PRIMARY KEY,
                    user_id UUID NOT NULL,
                    message TEXT NOT NULL,
                    type VARCHAR(50) NOT NULL,
                    sent_at TIMESTAMPTZ NOT NULL,
                    read BOOLEAN NOT NULL DEFAULT FALSE
                );

                CREATE INDEX IF NOT EXISTS idx_notifications_user_id ON notifications (user_id);
                """;

            await _dbContext.Connection.ExecuteAsync(createTableSql);
            _logger.LogInformation("Schema do banco de dados verificado para NotificationsService.");
        }
    }
}
