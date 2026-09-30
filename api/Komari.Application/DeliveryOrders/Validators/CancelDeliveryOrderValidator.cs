using FluentValidation;
using Komari.Application.DeliveryOrders.DTOs;

namespace Komari.Application.DeliveryOrders.Validators;

public class CancelDeliveryOrderValidator : AbstractValidator<CancelDeliveryOrderRequest>
{
    public CancelDeliveryOrderValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("A justificativa de cancelamento do pedido é obrigatória.")
            .Length(3, 500)
            .WithMessage("A justificativa de cancelamento deve ter entre 3 e 500 caracteres.");
    }
}
