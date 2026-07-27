using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Reservation.BookingsService.DAL.Enums;

namespace Reservation.BookingsService.DAL.Entities
{
    public class Booking
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }
        
        [BsonRepresentation(BsonType.String)]
        public Guid SpaceId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [BsonRepresentation(BsonType.String)]
        public BookingStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}