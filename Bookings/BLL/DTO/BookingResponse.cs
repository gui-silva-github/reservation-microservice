namespace Reservation.BookingsService.BLL.DTO;

public record BookingResponse(
    Guid Id,
    Guid UserId,
    Guid SpaceId,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    DateTime CreatedAt);
