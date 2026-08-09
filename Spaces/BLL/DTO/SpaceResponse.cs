namespace Reservation.SpacesService.BLL.DTO
{
    public record SpaceResponse(
        Guid Id,
        string Name,
        string Location,
        string? Description,
        int Capacity,
        decimal PricePerHour,
        bool IsActive
    );
}