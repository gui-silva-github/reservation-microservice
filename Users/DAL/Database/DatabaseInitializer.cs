using Dapper;
using Microsoft.Extensions.Logging;
using Reservation.UsersService.DAL.Enums;

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
            await SeedAdminAsync();
            _logger.LogInformation("Database schema verificado para UsersService.");
        }

        private async Task SeedAdminAsync() {
            const string email = "admin@reservas.com";
            int existing = await _dbContext.Connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM users WHERE email = @Email",
                new { Email = email });

            if (existing > 0) {
                return;
            }

            const string query = """
                INSERT INTO users (id, name, email, password_hash, role, created_at)
                VALUES (@Id, @Name, @Email, @PasswordHash, @Role, @CreatedAt)
            """;

            await _dbContext.Connection.ExecuteAsync(query, new
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Administrador",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin.ToString(),
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Usuário admin inicial criado: {Email}", email);
        }
    }
}
