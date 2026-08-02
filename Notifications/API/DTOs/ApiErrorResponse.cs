namespace Reservation.NotificationsService.API.DTOs
{
    public record ApiErrorResponse(
        string Message,
        IReadOnlyDictionary<string, string[]>? Errors = null,
        string? Type = null);
}
