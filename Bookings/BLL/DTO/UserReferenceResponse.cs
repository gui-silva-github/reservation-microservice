namespace Reservation.BookingsService.BLL.DTO
{
    public record UserReferenceResponse (
        Guid Id,
        string Name,
        string Email,
        string Role,
        DateTime CreatedAt
    );
}