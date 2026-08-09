namespace Reservation.SpacesService.BLL.DTO
{
    public record AvailabilityResponse(
        Guid SpaceId,
        string Name,
        DateTime From,
        DateTime To,
        bool IsAvailable,
        IReadOnlyList<BookingConflictResponse> Conflicts
    );

    public record BookingConflictResponse(
        Guid BookingId,
        DateTime StartDate,
        DateTime EndDate,
        string Status
    );
}