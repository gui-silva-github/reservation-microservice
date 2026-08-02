using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reservation.NotificationsService.BLL.Interfaces;
using Reservation.NotificationService.BLL.Mappers;
using Reservation.NotificationService.BLL.Services;
using Reservation.NotificationService.BLL.Validators;
using Reservation.NotificationsService.BLL.Workers;
using StackExchange.Redis;

namespace Reservation.NotificationsService.BLL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBLL(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(typeof(NotificationMappingProfile));
            services.AddValidatorsFromAssemblyContaining<SendNotificationRequestValidator>();

            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
               string? connectionString = configuration.GetSection("Redis:ConnectionString").Value;

               if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("Redis connection string não está configurada.");
                } 

                return ConnectionMultiplexer.Connect(connectionString);
            });

            services.AddScoped<INotificationService, NotificationService>();
            services.AddHostedService<BookingEventsConsumerWorker>();

            return services;
        }
    }
}
