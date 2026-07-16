namespace Reservation.BookingsService.BLL.Exceptions
{
    public class BookingConflictException : Exception
    {
        public BookingConflictException(string message) : base(message) {
        }
    }
}