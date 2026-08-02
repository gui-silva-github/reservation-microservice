using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Reservation.NotificationsService.DAL.Database
{
    public sealed class DapperDbContext : IDisposable
    {
        private readonly IDbConnection _connection;

        public DapperDbContext(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("PostgresConnection");
        
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'PostgresConnection' não está configurado.");
            }

            _connection = new NpgsqlConnection(connectionString);
        }

        public IDbConnection Connection
        {
            get
            {
                if (Connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                return _connection;
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
