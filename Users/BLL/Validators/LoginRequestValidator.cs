using FluentValidation;
using Reservation.UsersService.BLL.DTOs;

namespace Reservation.UsersService.BLL.Validators {
    public class LoginRequestValidator : AbstractValidator<LoginRequest> {
        public LoginRequestValidator() {
            RuleFor(request => request.Email)
                .NotEmpty()
                .EmailAddress();
            RuleFor(request => request.Password)
                .NotEmpty();
        }
    }
}