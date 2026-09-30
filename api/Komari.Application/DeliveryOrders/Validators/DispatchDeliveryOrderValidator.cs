using FluentValidation;
using Komari.Application.DeliveryOrders.DTOs;

namespace Komari.Application.DeliveryOrders.Validators;

public class DispatchDeliveryOrderValidator : AbstractValidator<DispatchDeliveryOrderRequest>
{
    public DispatchDeliveryOrderValidator()
    {
        RuleFor(x => x.DriverName)
            .NotEmpty()
            .WithMessage("O nome do entregador/motoboy é obrigatório para despacho.")
            .Length(2, 100)
            .WithMessage("O nome do entregador deve conter entre 2 e 100 caracteres.");
    }
}
