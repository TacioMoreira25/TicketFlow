using FluentValidation;
using TicketFlow.Application.DTOs;

namespace TicketFlow.Application.Validators;

public class BuyTicketValidator : AbstractValidator<BuyTicketRequest>
{
    public BuyTicketValidator()
    {
        RuleFor(request => request.TicketId)
            .NotEmpty().WithMessage("O TicketId é obrigatório.");

        RuleFor(request => request.OwnerName)
            .NotEmpty().WithMessage("O OwnerName é obrigatório.")
            .MinimumLength(3).WithMessage("O OwnerName deve ter no mínimo 3 caracteres.");
    }
}
