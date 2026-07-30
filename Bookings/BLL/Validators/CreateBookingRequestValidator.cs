using FluentValidation;
using Reservation.BookingsService.BLL.DTO;

namespace Reservation.BookingsService.BLL.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty().WithMessage("O identificador do usuário é obrigatório.");
        RuleFor(request => request.SpaceId)
            .NotEmpty().WithMessage("O identificador do espaço é obrigatório.");
        RuleFor(request => request.StartDate)
            .NotEmpty().WithMessage("A data de início é obrigatória.");
        RuleFor(request => request.EndDate)
            .NotEmpty().WithMessage("A data de término é obrigatória.")
            .GreaterThan(request => request.StartDate)
            .WithMessage("A data de término deve ser posterior à data de início.");
    }
}
