namespace Reservation.BookingsService.BLL.Exceptions
{
    public class InvalidReferenceException : Exception
    {
        public InvalidReferenceException(string message) : base(message)
        {
        }
    }
}