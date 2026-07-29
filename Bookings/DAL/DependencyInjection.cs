using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Reservation.BookingsService.DAL.Configuration;
using Reservation.BookingsService.DAL.Context;
using Reservation.BookingsService.DAL.Repositories;

namespace Reservation.BookingsService.DAL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

            services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));

            string connectionString = configuration.GetConnectionString("MongoConnection")
                ?? throw new InvalidOperationException("MongoDB connection string não está configurada.");

            services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
            services.AddSingleton<MongoDbContext>();
            services.AddScoped<IBookingRepository, BookingRepository>();

            return services;
        }
    }
}