namespace Reservation.NotificationsService.BLL.DTOs
{
    public record BookingCreatedEvent(
        string EventType,
        Guid BookingId,
        Guid UserId,
        Guid SpaceId,
        DateTime StartDate,
        DateTime EndDate);
}
