using Dapper;
using Microsoft.Extensions.Logging;

namespace Reservation.UsersService.DAL.Database {
    public sealed class DatabaseInitializer {
        private readonly DapperDbContext _dbContext;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(DapperDbContext dbContext, ILogger<DatabaseInitializer> logger) {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task InitializeAsync() {
            const string createTableSql = """
                CREATE TABLE IF NOT EXISTS users (
                    id UUID PRIMARY KEY,
                    name VARCHAR(120) NOT NULL,
                    email VARCHAR(255) NOT NULL UNIQUE,
                    password_hash VARCHAR(255) NOT NULL,
                    role VARCHAR(20) NOT NULL DEFAULT 'Customer',
                    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
                )
            """;

            await _dbContext.Connection.ExecuteAsync(createTableSql);
            _logger.LogInformation("Database schema verificado para UsersService.");
        }
    }
}