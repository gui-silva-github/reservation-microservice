using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reservation.NotificationsService.BLL.Constants;
using Reservation.NotificationsService.BLL.DTOs;
using Reservation.NotificationsService.BLL.Interfaces;
using StackExchange.Redis;

namespace Reservation.NotificationsService.BLL.Workers
{
    public sealed class BookingEventConsumerWorker : BackgroundService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<BookingEventsConsumerWorker> _logger;

        public BookingEventConsumerWorker(
            IServiceScopeFactory serviceScopeFactory,
            IConnectionMultiplexer redis,
            ILogger<BookingEventsConsumerWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _redis = redis;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Booking events consumer started. Listening on Redis queue '{{QueueKey}}'.",
                RedisQueueKeys.BookingEvents);

            IDatabase database = _redis.GetDatabase();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    RedisValue payload = await database.ListRightPopAsync(RedisQueueKeys.BookingEvents);

                    if (payload.IsNullOrEmpty)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                        continue;
                    }

                    await ProcessMessageAsync(payload.ToString(), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unexpected error while consuming booking events. Worker will continue.");
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }

            _logger.LogInformation("Booking events consumer stopped.");
        }

        private async Task ProcessMessageAsync(string payload, CancellationToken stoppingToken)
        {
            BookingCreatedEvent? bookingEvent;

            try
            {
                bookingEvent = JsonSerializer.Deserialize<BookingCreatedEvent>(payload, JsonOptions);
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "Invalid booking event payload received: {Payload}", payload);
                return;
            }

            if (bookingEvent is null)
            {
                _logger.LogWarning("Empty booking event payload received.");
                return;
            }

            if (!string.Equals(bookingEvent.EventType, BookingEventTypes.BookingCreated, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Ignoring unsupported booking event type '{EventType}'.", bookingEvent.EventType);
                return;
            }

            try
            {
                using IServiceScope scope = _serviceScopeFactory.CreateScope();
                INotificationService notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await notificationService.ProcessBookingCreatedEventAsync(bookingEvent);

                _logger.LogInformation(
                    "Notification persisted for booking {BookingId} and user {UserId}.",
                    bookingEvent.BookingId,
                    bookingEvent.UserId);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to persist notification for booking {BookingId}. Worker will continue processing.",
                    bookingEvent.BookingId);
            }
        }
    }
}
