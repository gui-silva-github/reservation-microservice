namespace Reservation.SpacesService.BLL.DTO
{
    public record UpdateSpaceRequest(
        string Name,
        string Location,
        string? Description,
        int Capacity,
        decimal PricePerHour
    );
}