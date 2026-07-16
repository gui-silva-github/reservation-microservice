namespace Reservation.BookingsService.BLL.DTO
{
    public record BookingResponse(
        Guid Id,
        Guid UserId,
        DateTime StartDate,
        DateTime EndDate,
        string Status,
        DateTime CreatedAt
    );
}