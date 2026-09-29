using FluentValidation;
using Komari.Application.Products.DTOs;

namespace Komari.Application.Products.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome do produto é obrigatório.")
            .MaximumLength(150).WithMessage("O nome do produto não pode ultrapassar 150 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("O preço do produto deve ser maior que zero.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("A categoria informada é obrigatória.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("A descrição não pode ultrapassar 1000 caracteres.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2000).WithMessage("A URL da imagem não pode ultrapassar 2000 caracteres.");
    }
}
