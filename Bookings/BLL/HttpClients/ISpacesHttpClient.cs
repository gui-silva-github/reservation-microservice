using Reservation.BookingsService.BLL.DTO;

namespace Reservation.BookingsService.BLL.HttpClients
{
    public interface ISpacesHttpClient
    {
        Task<SpaceReferenceResponse?> GetSpaceByIdAsync(Guid spaceId, CancellationToken cancellationToken = default);
    }
}