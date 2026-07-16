namespace Reservation.BookingsService.BLL.Exceptions
{
    public class ExternalServiceUnavailableException : Exception
    {
        public ExternalServiceUnavailableException(string message) : base(message)
        {
        }
    }
}