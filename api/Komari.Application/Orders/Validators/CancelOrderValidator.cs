using FluentValidation;
using Komari.Application.Orders.DTOs;

namespace Komari.Application.Orders.Validators;

public class CancelOrderValidator : AbstractValidator<CancelOrderRequest>
{
    public CancelOrderValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("O motivo do cancelamento é obrigatório.")
            .MaximumLength(500)
            .WithMessage("O motivo do cancelamento não pode exceder 500 caracteres.");
    }
}
