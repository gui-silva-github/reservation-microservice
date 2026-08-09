using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reservation.SpacesService.BLL.DTO;
using Reservation.SpacesService.BLL.Exceptions;
using Reservation.SpacesService.BLL.Options;

namespace Reservation.SpacesService.BLL.HttpClients
{
    public class BookingsHttpClient : IBookingsHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BookingsHttpClient> _logger;

        public BookingsHttpClient(
            HttpClient httpClient,
            IOptions<BookingsServiceOptions> options,
            ILogger<BookingsHttpClient> logger
        )
        {
            _httpClient = httpClient;
            _logger = logger;

            if (!string.IsNullOrWhiteSpace(options.Value.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(options.Value.BaseUrl.TrimEnd('/') + "/");
            }
        }

        public async Task<IReadOnlyList<BookingResponse>> GetBookingsBySpaceIdAsync(
            Guid spaceId,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(
                    $"api/bookings/search/spaceid/{spaceId}",
                    cancellationToken
                );

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return Array.Empty<BookingResponse>();
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "BookingsService retornou status {StatusCode} ao buscar reservas do espaço {SpaceId}.",
                        response.StatusCode,
                        spaceId
                    );

                    throw new ExternalServiceUnavailableException(
                        "Não foi possível consultar as reservas no momento. Tente novamente mais tarde."
                    );
                }

                List<BookingResponse>? bookings = await response.Content.ReadFromJsonAsync<List<BookingResponse>>(
                    cancellationToken: cancellationToken
                );

                return bookings is null ? Array.Empty<BookingResponse>() : bookings;
            } catch (ExternalServiceUnavailableException)
            {
                throw;
            } catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Falha ao consultar reservas do espaço {SpaceId} no BookingsService.",
                    spaceId
                );

                throw new ExternalServiceUnavailableException(
                    "Não foi possível consultar as reservas no momento. Tente novamente mais tarde;"
                );
            }
        }
    }
}