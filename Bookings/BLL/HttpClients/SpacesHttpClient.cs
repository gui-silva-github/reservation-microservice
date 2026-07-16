using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reservation.BookingsService.BLL.DTO;
using Reservation.BookingsService.BLL.Exceptions;
using Reservation.BookingsService.BLL.Options;

namespace Reservation.BookingsService.BLL.HttpClients
{
    public class SpacesHttpClient : ISpacesHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SpacesHttpClient> _logger;

        public SpacesHttpClient(
            HttpClient httpClient,
            IOptions<SpacesServiceOptions> options,
            ILogger<SpacesHttpClient> logger
        )
        {
            _httpClient = httpClient;
            _logger = logger;

            if (!string.IsNullOrWhiteSpace(options.Value.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(options.Value.BaseUrl.TrimEnd('/') + "/");
            }
        }

        public async Task<SpaceReferenceResponse?> GetSpaceByIdAsync(Guid spaceId, CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(
                    $"api/spaces/{spaceId}",
                    cancellationToken
                );

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "SpacesService retornou status {StatusCode} ao buscar espaço {SpaceId}.",
                        response.StatusCode,
                        spaceId
                    );

                    throw new ExternalServiceUnavailableException(
                        "Não foi possível validar o espaço no momento. Tente novamente mais tarde."
                    );
                }

                return await response.Content.ReadFromJsonAsync<SpaceReferenceResponse>(cancellationToken: cancellationToken);
            } catch (ExternalServiceUnavailableException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Falha ao consultar espaço {SpaceId} no SpacesService.",
                    spaceId
                );   

                throw new ExternalServiceUnavailableException(
                    "Não foi possível validar o espaço no momento. Tente novamente mais tarde.");
            }
        }
    }
}