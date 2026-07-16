using System.Text.Json;
using Microsoft.Extensions.Logging;
using Reservation.BookingsService.BLL.Constants;
using Reservation.BookingsService.BLL.DTO;
using StackExchange.Redis;

namespace Reservation.BookingsService.BLL.Messaging
{
    public class RedisEventPublisher : IEventPublisher
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly ILogger<RedisEventPublisher> _logger;

        public RedisEventPublisher(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisEventPublisher> logger)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _logger = logger;
        }

        public async Task PublishBookingCreatedAsync(
            BookingCreatedEvent bookingCreatedEvent,
            CancellationToken cancellationToken = default
        )
        {
            string payload = JsonSerializer.Serialize(bookingCreatedEvent, JsonOptions);
            IDatabase database = _connectionMultiplexer.GetDatabase();

            await database.ListLeftPushAsync(RedisQueueConstants.BookingEventsQueue, payload)
                .WaitAsync(cancellationToken);

            _logger.LogInformation(
                "Evento {EventType} publicado na fila {QueueName} para a reserva {BookingId}.",
                RedisQueueConstants.BookingCreatedEventType,
                RedisQueueConstants.BookingEventsQueue,
                bookingCreatedEvent.BookingId
            );
        }
    }
}