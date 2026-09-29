using FluentValidation;
using Komari.Application.Orders.DTOs;

namespace Komari.Application.Orders.Validators;

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("O status do pedido informado é inválido.");

        RuleFor(x => x.CancellationReason)
            .MaximumLength(500)
            .WithMessage("A justificativa de cancelamento não pode exceder 500 caracteres.");
    }
}
