namespace Reservation.BookingsService.DAL.Configuration
{
    public class MongoDbSettings
    {
        public const string SectionName = "MongoDb";

        public string DatabaseName { get; set; } = "ReservationBookings";
    }
}