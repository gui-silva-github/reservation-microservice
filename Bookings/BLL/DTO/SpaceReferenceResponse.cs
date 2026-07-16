namespace Reservation.BookingsService.BLL.DTO
{
    public record SpaceReferenceResponse(
        Guid Id,
        string Name,
        string Location,
        string? Description,
        int Capacity,
        decimal PricePerHour,
        bool IsActive
    );
}