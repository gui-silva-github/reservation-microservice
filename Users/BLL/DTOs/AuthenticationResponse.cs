using Reservation.UsersService.DAL.Enums;

namespace Reservation.UsersService.BLL.DTOs {
    public record AuthenticationResponse(
        bool Success,
        string Token,
        Guid UserId,
        string Name,
        string Email,
        UserRole Role
    );
}