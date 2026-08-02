using Microsoft.Extensions.DependencyInjection;
using Reservation.NotificationsService.DAL.Database;
using Reservation.NotificationsService.DAL.Repositories;

namespace Reservation.NotificationsService.DAL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDAL(this IServiceCollection services)
        {
            services.AddScoped<DapperDbContext>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<DatabaseInitializer>();

            return services;
        }
    }
}
