using FluentValidation;
using Komari.Application.Orders.DTOs;

namespace Komari.Application.Orders.Validators;

public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("O identificador do produto (ProductId) é obrigatório.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("A quantidade do item deve ser maior que zero.");

        RuleFor(x => x.Notes)
            .MaximumLength(300)
            .WithMessage("As observações do item não podem exceder 300 caracteres.");

        RuleFor(x => x.Size)
            .MaximumLength(50)
            .WithMessage("O tamanho do item não pode exceder 50 caracteres.");
    }
}
