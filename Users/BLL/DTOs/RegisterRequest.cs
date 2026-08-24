using Reservation.UsersService.DAL.Enums;

namespace Reservation.UsersService.BLL.DTOs {
    public record RegisterRequest(string Name, string Email, string Password);
}