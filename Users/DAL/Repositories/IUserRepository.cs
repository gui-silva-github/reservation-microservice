using Reservation.UsersService.DAL.Entities;

namespace Reservation.UsersService.DAL.Repositories {
    public interface IUserRepository {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
    }
}