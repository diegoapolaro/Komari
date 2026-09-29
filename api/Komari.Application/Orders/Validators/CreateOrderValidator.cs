using FluentValidation;
using Komari.Application.Orders.DTOs;

namespace Komari.Application.Orders.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.BillId)
            .NotEmpty()
            .WithMessage("O identificador da comanda (BillId) é obrigatório.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("O pedido deve conter pelo menos um item de consumo.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemValidator());

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("As observações do pedido não podem exceder 500 caracteres.");
    }
}
