using FluentValidation;
using Komari.Application.Categories.DTOs;

namespace Komari.Application.Categories.Validators;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome da categoria é obrigatório.")
            .MaximumLength(100).WithMessage("O nome da categoria não pode ultrapassar 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("A descrição não pode ultrapassar 500 caracteres.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("A ordem de exibição deve ser maior ou igual a zero.");
    }
}
