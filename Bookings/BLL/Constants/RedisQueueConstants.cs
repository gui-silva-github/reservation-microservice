namespace Reservation.BookingsService.BLL.Constants
{
    public static class RedisQueueConstants
    {
        public const string BookingEventsQueue = "booking-events";
        public const string BookingCreatedEventType = "BookingCreated";
    }
}