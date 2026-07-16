namespace Reservation.BookingsService.BLL.DTO
{
    public record BookingCreatedEvent(
        string EventType,
        Guid BookingId,
        Guid UserId,
        Guid SpaceId,
        DateTime StartDate,
        DateTime EndDate
    );
}