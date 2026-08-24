namespace Reservation.UsersService.BLL.Exceptions {
    public class InvalidCredentialsException : Exception {
        public InvalidCredentialsException() : base("Email e/ou senha incorretos.") {
        }
    }
}
