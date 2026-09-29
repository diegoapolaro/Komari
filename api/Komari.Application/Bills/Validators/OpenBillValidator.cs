using FluentValidation;
using Komari.Application.Bills.DTOs;

namespace Komari.Application.Bills.Validators;

public class OpenBillValidator : AbstractValidator<OpenBillRequest>
{
    public OpenBillValidator()
    {
        RuleFor(x => x.Number)
            .GreaterThan(0)
            .When(x => x.Number.HasValue)
            .WithMessage("O número da comanda deve ser maior que zero.");

        RuleFor(x => x.CounterName)
            .MaximumLength(100)
            .WithMessage("O nome do balcão não pode exceder 100 caracteres.");

        RuleFor(x => x.CustomerName)
            .MaximumLength(100)
            .WithMessage("O nome do cliente não pode exceder 100 caracteres.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("As observações não podem exceder 500 caracteres.");
    }
}
