using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reservation.SpacesService.DAL.Context;
using Reservation.SpacesService.DAL.Repositories;

namespace Reservation.SpacesService.DAL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionStringTemplate = configuration.GetConnectionString("DefaultConnection")!;
            string connectionString = connectionStringTemplate
                .Replace("$MYSQL_HOST", Environment.GetEnvironmentVariable("MYSQL_HOST") ?? "localhost")
                .Replace("$MYSQL_PASSWORD", Environment.GetEnvironmentVariable("MYSQL_PASSWORD") ?? "admin");

            services.AddDbContext<SpacesDbContext>(options =>
            {
                options.UseMySQL(connectionString);
            });

            services.AddScoped<ISpaceRepository, SpaceRepository>();

            return services;
        }
    }
}