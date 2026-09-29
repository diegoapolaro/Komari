using FluentValidation;
using Komari.Application.Bills.DTOs;

namespace Komari.Application.Bills.Validators;

public class CancelBillValidator : AbstractValidator<CancelBillRequest>
{
    public CancelBillValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("O motivo do cancelamento é obrigatório.")
            .MaximumLength(300)
            .WithMessage("O motivo do cancelamento não pode exceder 300 caracteres.");
    }
}
