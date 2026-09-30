using FluentValidation;
using Komari.Application.DeliveryOrders.DTOs;
using Komari.Application.Orders.Validators;
using Komari.Domain.Enums;

namespace Komari.Application.DeliveryOrders.Validators;

public class CreateDeliveryOrderValidator : AbstractValidator<CreateDeliveryOrderRequest>
{
    public CreateDeliveryOrderValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("O identificador do cliente (CustomerId) é obrigatório.");

        RuleFor(x => x.CustomerAddressId)
            .NotEmpty()
            .WithMessage("O identificador do endereço de entrega (CustomerAddressId) é obrigatório.");

        RuleFor(x => x.DeliveryFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage("A taxa de entrega não pode ser negativa.");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O desconto não pode ser negativo.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("A forma de pagamento informada é inválida.");

        RuleFor(x => x.ChangeFor)
            .Must((request, changeFor) => !changeFor.HasValue || request.PaymentMethod == DeliveryPaymentMethod.Cash)
            .WithMessage("O valor de troco só pode ser informado para pagamentos em dinheiro.")
            .Must((request, changeFor) => !changeFor.HasValue || changeFor.Value > 0)
            .WithMessage("O valor para troco deve ser maior que zero quando informado.");

        RuleFor(x => x.EstimatedMinutes)
            .GreaterThan(0)
            .When(x => x.EstimatedMinutes.HasValue)
            .WithMessage("O tempo estimado de entrega deve ser maior que zero minutos.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("O pedido de delivery deve conter pelo menos um item de consumo.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemValidator());

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("As observações do pedido não podem exceder 500 caracteres.");
    }
}
