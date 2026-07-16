using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reservation.BookingsService.BLL.DTO;
using Reservation.BookingsService.BLL.Exceptions;
using Reservation.BookingsService.BLL.Options;

namespace Reservation.BookingsService.BLL.HttpClients
{
    public class UsersHttpClient : IUsersHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UsersHttpClient> _logger;

        public UsersHttpClient(
            HttpClient httpClient,
            IOptions<UsersServiceOptions> options,
            ILogger<UsersHttpClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            if (!string.IsNullOrWhiteSpace(options.Value.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(options.Value.BaseUrl.TrimEnd('/') + "/");
            }
        }

        public async Task<UserReferenceResponse?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(
                    $"api/Users/{userId}",
                    cancellationToken
                );

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "UsersService retornou status {StatusCode} ao buscar usuário {UserId}.",
                        response.StatusCode,
                        userId
                    );

                    throw new ExternalServiceUnavailableException(
                        "Não foi possível validar o usuário no momento. Tente novamente mais tarde.");
                }

                return await response.Content.ReadFromJsonAsync<UserReferenceResponse>(cancellationToken: cancellationToken);
            }
            catch (ExternalServiceUnavailableException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Falha ao consultar usuário {UserId} no UsersService.",
                    userId);

                throw new ExternalServiceUnavailableException(
                    "Não foi possível validar o usuário no momento. Tente novamente mais tarde.");
            }
        }
    }
}