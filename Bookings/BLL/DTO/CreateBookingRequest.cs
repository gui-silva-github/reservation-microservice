namespace Reservation.BookingsService.BLL.DTO
{
    public record CreateBookingRequest(
        Guid UserId,
        Guid SpaceId,
        DateTime StartDate,
        DateTime EndDate
    );
}