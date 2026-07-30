using Reservation.BookingsService.DAL.Enums;

namespace Reservation.BookingsService.BLL.DTO;

public record UpdateBookingRequest(
    Guid UserId,
    Guid SpaceId,
    DateTime StartDate,
    DateTime EndDate,
    BookingStatus Status);
