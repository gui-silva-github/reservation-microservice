using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reservation.SpacesService.BLL.HttpClients;
using Reservation.SpacesService.BLL.Mappers;
using Reservation.SpacesService.BLL.Options;
using Reservation.SpacesService.BLL.Services;
using Reservation.SpacesService.BLL.Validators;

namespace Reservation.SpacesService.BLL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLogicLayer(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.Configure<BookingsServiceOptions>(options =>
            {
               options.BaseUrl = configuration["BookingsServiceUrl"] ?? string.Empty; 
            });

            services.AddAutoMapper(typeof(CreateSpaceRequestToSpaceMappingProfile).Assembly);
            services.AddValidatorsFromAssemblyContaining<CreateSpaceRequestValidator>();

            services.AddHttpClient<IBookingsHttpClient, BookingsHttpClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);   
            });

            services.AddScoped<ISpaceService, SpaceService>();

            return services;
        }
    }
}