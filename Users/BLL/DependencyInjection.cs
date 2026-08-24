using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Reservation.UsersService.BLL.Interfaces;
using Reservation.UsersService.BLL.Mappings;
using Reservation.UsersService.BLL.Services;
using Reservation.UsersService.BLL.Validators;

namespace Reservation.UsersService.BLL {
    public static class DependencyInjection
    {
        public static IServiceCollection AddBLL(this IServiceCollection services) {
            services.AddAutoMapper(typeof(UserMappingProfile));
            services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            return services;
        }
    }
}