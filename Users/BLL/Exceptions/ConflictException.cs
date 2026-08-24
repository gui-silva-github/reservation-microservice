namespace Reservation.UsersService.BLL.Exceptions {
    public class ConflictException : Exception {
        public ConflictException(string message) : base(message) {
        }
    }
}