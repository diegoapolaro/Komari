using FluentValidation;
using Komari.Application.Payments.DTOs;
using Komari.Domain.Enums;

namespace Komari.Application.Payments.Validators;

public class RegisterPaymentValidator : AbstractValidator<RegisterPaymentRequest>
{
    public RegisterPaymentValidator()
    {
        RuleFor(x => x.BillId)
            .NotEmpty()
            .WithMessage("O identificador da comanda é obrigatório.");

        RuleFor(x => x.Method)
            .IsInEnum()
            .WithMessage("Forma de pagamento inválida.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("O valor do pagamento deve ser maior que zero.");

        RuleFor(x => x.AmountTendered)
            .GreaterThanOrEqualTo(x => x.Amount)
            .WithMessage("Para pagamento em dinheiro, o valor entregue deve ser maior ou igual ao valor da parcela.")
            .When(x => x.Method == PaymentMethod.Cash);

        RuleFor(x => x.AmountTendered)
            .NotNull()
            .WithMessage("Para pagamento em dinheiro, o valor entregue (AmountTendered) é obrigatório.")
            .When(x => x.Method == PaymentMethod.Cash);

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("As observações do pagamento devem ter no máximo 500 caracteres.")
            .When(x => x.Notes != null);
    }
}
