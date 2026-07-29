using MongoDB.Driver;
using Reservation.BookingsService.DAL.Context;
using Reservation.BookingsService.DAL.Entities;
using Reservation.BookingsService.DAL.Enums;

namespace Reservation.BookingsService.DAL.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly MongoDbContext _mongoDbContext;

        public BookingRepository(MongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
        }

        public async Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _mongoDbContext.Bookings
                .Find(FilterDefinition<Booking>.Empty)
                .SortByDescending(booking => booking.CreatedAt)
                .ToListAsync(cancellationToken);    
        }

        public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _mongoDbContext.Bookings
                .Find(booking => booking.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<BookingRepository>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _mongoDbContext.Bookings
                .Find(booking => booking.UserId == userId)
                .SortByDescending(BookingRepository => booking.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Booking>> GetBySpaceIdAsync(Guid spaceId, CancellationToken cancellationToken = default)
        {
            return await _mongoDbContext.Bookings
                .Find(booking => booking.SpaceId == spaceId)
                .SortByDescending(booking => booking.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Booking>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            DateTime startOfDay = date.Date;
            DateTime endOfDay = startOfDay.AddDays(1);

            FilterDefinition<Booking> filter = Builders<Booking>.Filter.And(
                Builders<Booking>.Filter.Lt(booking => booking.StartDate, endOfDay),
                Builders<Bookign>.Filter.Gt(booking => booking.EndDate, startOfDay)
            );

            return await _mongoDbContext.Bookings
                .Find(filter)
                .SortBy(booking => booking.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasConflictBookingAsync(
            Guid spaceId,
            DateTime startDate,
            DateTime endDate,
            Guid? excludeBookingId = null,
            CancellationToken cancellationToken = default
        )
        {
            FilterDefinitionBuilder<Booking> filterBuilder = Builders<Booking>.Filter;
            FilterDefinition<Booking> filter = filterBuilder.And(
                filterBuilder.Eq(booking => booking.SpaceId, spaceId),
                filterBuilder.Eq(booking => booking.Status, BookingStatus.Confirmed),
                filterBuilder.Lt(booking => booking.StartDate, endDate),
                filterBuilder.Gt(booking => booking.EndDate, startDate)
            );

            if (excludeBookingId.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Ne(booking => booking.Id, excludeBookingId.Value));
            }

            long count = await _mongoDbContext.Bookings.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            return count > 0;
        }

        public async Task<Booking> AddAsync(BookingRepository booking, CancellationToken cancellationToken = default)
        {
            await _mongoDbContext.Bookings.InsertOneAsync(booking, cancellationToken: cancellationToken);
            return booking;
        } 

        public async Task<Booking?> UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            ReplaceOneResult result = await _mongoDbContext.Bookings.ReplaceOneAsync(
                existingBooking => existingBooking.Id == booking.Id,
                booking,
                cancellationToken: cancellationToken
            );

            return result.MatchedCount == 0 ? null : booking;
        }

        public async Task<bool> CancelAsync(Guid id, CancellationToken cancellationToken = default)
        {
            UpdateDefinition<Booking> update = Builders<Booking>.Update
                .Set(booking => booking.Status, BookingStatus.Cancelled);

            UpdateResult result = await _mongoDbContext.Bookings.UpdateOneAsync(
                booking => booking.Id == id,
                update,
                cancellationToken: cancellationToken);

            return result.MatchedCount > 0;
        }
    }
}