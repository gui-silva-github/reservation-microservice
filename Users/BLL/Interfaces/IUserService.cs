using Reservation.UsersService.BLL.DTOs;

namespace Reservation.UsersService.BLL.Interfaces {
    public interface IUserService {
        Task<UserResponse> GetByIdAsync(Guid id);
    }
}