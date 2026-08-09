using FluentValidation;
using Reservation.SpacesService.BLL.DTO;

namespace Reservation.SpacesService.BLL.Validators
{
    public class CreateSpaceRequestValidator : AbstractValidator<CreateSpaceRequest>
    {
        public CreateSpaceRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty().WithMessage("O nome do espaço é obrigatório.")
                .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres");
            RuleFor(request => request.Location)
                .NotEmpty().WithMessage("A localização é obrigatória.")
                .MaximumLength(200).WithMessage("A localização deve ter no máximo 200 caracteres.");
            RuleFor(request => request.Description)
                .MaximumLength(2000).WithMessage("A descrição deve ter no máximo 2000 caracteres");
            RuleFor(request => request.Capacity)
                .GreaterThan(0).WithMessage("A capacidade deve ser maior que zero.");
            RuleFor(request => request.PricePerHour)
                .GreaterThanOrEqualTo(0).WithMessage("O preço por hora não pode ser negativo.");
        }
    }
}