namespace Reservation.SpacesService.BLL.DTO
{
    public record CreateSpaceRequest(
        string Name,
        string Location,
        string? Description,
        int Capacity,
        decimal PricePerHour
    );
}