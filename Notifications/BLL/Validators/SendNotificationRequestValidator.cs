using FluentValidation;
using Reservation.NotificationsService.BLL.DTOs;

namespace Reservation.NotificationsService.BLL.Validators
{
    public class SendNotificationRequestValidator : AbstractValidator<SendNotificationRequest>
    {
        public SendNotificationRequestValidator()
        {
            RuleFor(request => request.UserId)
                .NotEmpty();

            RuleFor(request => request.Message)
                .NotEmpty()
                .MaximumLength(2000);

            RuleFor(request => request.Type)
                .IsInEnum();
        }
    }
}
