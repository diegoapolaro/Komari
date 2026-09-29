using FluentValidation;
using Komari.Application.CashSessions.DTOs;
using Komari.Domain.Enums;

namespace Komari.Application.CashSessions.Validators;

public class AddCashMovementValidator : AbstractValidator<AddCashMovementRequest>
{
    public AddCashMovementValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Tipo de movimentação inválido.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("O valor da movimentação deve ser maior que zero.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("A descrição da movimentação é obrigatória.")
            .MaximumLength(500)
            .WithMessage("A descrição deve ter no máximo 500 caracteres.");
    }
}
