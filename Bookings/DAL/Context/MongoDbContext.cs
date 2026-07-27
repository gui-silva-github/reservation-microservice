using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Reservation.BookingsService.DAL.Configuration;
using Reservation.BookingsService.DAL.Constants;
using Reservation.BookingsService.DAL.Entities;

namespace Reservation.BookingsService.DAL.Context
{
    public class MongoDbContext
    {
        public MongoDbContext(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
        {
            IMongoDatabase database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            Bookings = database.GetCollection<Booking>(MongoDBConstants.BookingsCollection);
        }

        public IMongoCollection<Booking> Bookings { get; }
    }
}