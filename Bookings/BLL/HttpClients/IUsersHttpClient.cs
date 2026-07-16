using Reservation.BookingsService.BLL.DTO;

namespace Reservation.BookingsService.BLL.HttpClients
{
    public interface IUsersHttpClient
    {
        Task<UserReferenceResponse?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}