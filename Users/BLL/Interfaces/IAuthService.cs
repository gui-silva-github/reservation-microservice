using Reservation.UsersService.BLL.DTOs;

namespace Reservation.UsersService.BLL.Interfaces {
    public interface IAuthService {
        Task<AuthenticationResponse> RegisterAsync(RegisterRequest request);
        Task<AuthenticationResponse> LoginAsync(LoginRequest request);
    }
}