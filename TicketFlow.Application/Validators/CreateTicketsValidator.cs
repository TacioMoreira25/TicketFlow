using FluentValidation;
using TicketFlow.Application.DTOs;

namespace TicketFlow.Application.Validators;

public class CreateTicketsValidator : AbstractValidator<CreateTicketsRequest>
{
    public CreateTicketsValidator()
    {
        RuleFor(request => request.Quantity)
            .GreaterThan(0).WithMessage("A Quantity deve ser maior que 0.");

        RuleFor(request => request.Price)
            .GreaterThan(0).WithMessage("O Price deve ser maior que 0.");
    }
}
