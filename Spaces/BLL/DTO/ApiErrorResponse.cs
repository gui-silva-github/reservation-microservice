namespace Reservation.SpacesService.BLL.DTO
{
    public record ApiErrorResponse(
        string Error,
        string Message,
        IReadOnlyDictionary<string, string[]>? Details = null
    );
}