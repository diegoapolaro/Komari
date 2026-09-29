using FluentValidation;
using Komari.Application.CashSessions.DTOs;

namespace Komari.Application.CashSessions.Validators;

public class OpenCashSessionValidator : AbstractValidator<OpenCashSessionRequest>
{
    public OpenCashSessionValidator()
    {
        RuleFor(x => x.InitialAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O fundo de troco inicial não pode ser negativo.");

        RuleFor(x => x.OperatorName)
            .MaximumLength(200)
            .WithMessage("O nome do operador deve ter no máximo 200 caracteres.")
            .When(x => x.OperatorName != null);
    }
}
