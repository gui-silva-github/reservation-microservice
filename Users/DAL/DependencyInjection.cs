using Microsoft.Extensions.DependencyInjection;
using Reservation.UsersService.DAL.Database;
using Reservation.UsersService.DAL.Repositories;

namespace Reservation.UsersService.DAL {
    public static class DependencyInjection {
        public static IServiceCollection AddDAL(this IServiceCollection services) {
            services.AddScoped<DapperDbContext>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<DatabaseInitializer>();

            return services;
        }
    }
}