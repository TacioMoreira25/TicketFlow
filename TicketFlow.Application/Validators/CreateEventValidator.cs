using FluentValidation;
using TicketFlow.Application.DTOs;

namespace TicketFlow.Application.Validators
{
    public class CreateEventValidator : AbstractValidator<CreateEventRequest>
    {
        public CreateEventValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .Length(3, 100);

            RuleFor(x => x.Date)
                .NotEmpty();

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(500);
        }
    }
}