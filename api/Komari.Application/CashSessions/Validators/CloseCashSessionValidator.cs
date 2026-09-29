using FluentValidation;
using Komari.Application.CashSessions.DTOs;

namespace Komari.Application.CashSessions.Validators;

public class CloseCashSessionValidator : AbstractValidator<CloseCashSessionRequest>
{
    public CloseCashSessionValidator()
    {
        RuleFor(x => x.DeclaredAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O valor declarado na conferência não pode ser negativo.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("As observações de fechamento devem ter no máximo 1000 caracteres.")
            .When(x => x.Notes != null);
    }
}
