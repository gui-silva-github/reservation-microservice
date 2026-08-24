using Reservation.UsersService.DAL.Enums;

namespace Reservation.UsersService.BLL.DTOs {
    public record UserResponse(
        Guid Id,
        string Name,
        string Email,
        UserRole Role,
        DateTime CreatedAt
    );
}