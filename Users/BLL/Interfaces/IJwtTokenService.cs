using Reservation.UsersService.DAL.Entities;

namespace Reservation.UsersService.BLL.Interfaces {
    public interface IJwtTokenService {
        string GenerateToken(User user);
    }
}