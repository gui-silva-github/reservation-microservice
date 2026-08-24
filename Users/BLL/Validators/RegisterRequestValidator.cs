using FluentValidation;
using Reservation.UsersService.BLL.DTOs;

namespace Reservation.UsersService.BLL.Validators {
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest> {
        public RegisterRequestValidator() {
            RuleFor(request => request.Name)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(120);

            RuleFor(request => request.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(255);

            RuleFor(request => request.Password)
                .NotEmpty()
                .MinimumLength(6)
                .MaximumLength(100);
        }
    }
}