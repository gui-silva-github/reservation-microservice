using Dapper;
using Npgsql;
using Reservation.UsersService.DAL.Database;
using Reservation.UsersService.DAL.Entities;

namespace Reservation.UsersService.DAL.Repositories {
    internal sealed class UserRepository : IUserRepository {
        private const string SelectColumns = """
            id AS Id,
            name AS Name,
            email AS Email,
            password_hash AS PasswordHash,
            role AS Role,
            created_at AS CreatedAt
        """;

        private readonly DapperDbContext _dbContext;

        public UserRepository(DapperDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdAsync(Guid id) {
            string query = $"""
                SELECT {SelectColumns}
                FROM users
                WHERE id = @Id
            """;

            return await _dbContext.Connection.QueryFirstOrDefaultAsync<User>(query, new { Id = id });
        }

        public async Task<User?> GetByEmailAsync(string email) {
            string query = $"""
                SELECT {SelectColumns}
                FROM users
                WHERE email = @Email
            """;

            return await _dbContext.Connection.QueryFirstOrDefaultAsync<User>(query, new { Email = email });
        }

        public async Task<User> CreateAsync(User user) {
            const string query = """
                INSERT INTO users (id, name, email, password_hash, role, created_at)
                VALUES (@Id, @Name, @Email, @PasswordHash, @Role, @CreatedAt)
            """;

            await _dbContext.Connection.ExecuteAsync(query, new
            {
                user.Id,
                user.Name,
                user.Email,
                user.PasswordHash,
                Role = user.Role.ToString(),
                user.CreatedAt
            });

            return user;
        }
    }
}