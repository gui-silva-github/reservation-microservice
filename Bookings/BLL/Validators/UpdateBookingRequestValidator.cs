using FluentValidation;
using Reservation.BookingsService.BLL.DTO;
using Reservation.BookingsService.DAL.Enums;

namespace Reservation.BookingsService.BLL.Validators
{
    public class UpdateBookingRequestValidator : AbstractValidator<UpdateBookingRequest>
    {
        public UpdateBookingRequestValidator()
        {
            RuleFor(request => request.UserId)
                .NotEmpty().WithMessage("O identificador de usuário é obrigatório.");
            RuleFor(request => request.SpaceId)
                .NotEmpty().WithMessage("O identificador do espaço é obrigatório.");
            RuleFor(request => request.StartDate)
                .NotEmpty().WithMessage("A data de início é obrigatória.");
            RuleFor(request => request.EndDate)
                .NotEmpty().WithMessage("A data de término é obrigatória.")
                .GreaterThan(request => request.StartDate)
                .WithMessage("A data de término deve ser posterior à data de início.");
            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("O status informado é inválido.")
                .NotEqual(BookingStatus.Cancelled)
                .WithMessage("Use o endpoint DELETE para cancelar uma reserva.");
        }
    }
}