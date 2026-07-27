using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reservation.BookingsService.BLL.HttpClients;
using Reservation.BookingsService.BLL.Mappers;
using Reservation.BookingsService.BLL.Messaging;
using Reservation.BookingsService.BLL.Options;
using Reservation.BookingsService.BLL.Services;
using Reservation.BookingsService.BLL.Validators;
using StackExchange.Redis;

namespace Reservation.BookingsService.BLL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLogicLayer(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.Configure<UsersServiceOptions>(options =>
            {
               options.BaseUrl = configuration["UsersServiceUrl"] ?? string.Empty; 
            });

            services.Configure<SpacesServiceOptions>(options =>
            {
                options.BaseUrl = configuration["SpacesServiceUrl"] ?? string.Empty;
            });

            string redisConnectionString = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Redis connection string não está configurado.");

            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));

            services.AddAutoMapper(typeof(BookingToBookingResponseMappingProfile).Assembly);
            services.AddValidatorsFromAssemblyContaining<CreateBookingRequestValidator>();

            services.AddHttpClient<IUsersHttpClient, UsersHttpClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            services.AddHttpClient<ISpacesHttpClient, SpacesHttpClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            services.AddSingleton<IEventPublisher, RedisEventPublisher>();
            services.AddScoped<IBookingService, BookingService>();

            return services;
        }
    }
}