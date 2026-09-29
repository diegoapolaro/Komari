using FluentValidation;
using Komari.Application.Customers.DTOs;

namespace Komari.Application.Customers.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome do cliente é obrigatório.")
            .Length(2, 150).WithMessage("O nome do cliente deve ter entre 2 e 150 caracteres.");

        When(x => !string.IsNullOrWhiteSpace(x.Phone), () =>
        {
            RuleFor(x => x.Phone!)
                .MinimumLength(8).WithMessage("O telefone do cliente deve ter no mínimo 8 dígitos.")
                .MaximumLength(20).WithMessage("O telefone do cliente não pode ultrapassar 20 caracteres.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email!)
                .EmailAddress().WithMessage("O e-mail informado não é válido.")
                .MaximumLength(150).WithMessage("O e-mail não pode ultrapassar 150 caracteres.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Document), () =>
        {
            RuleFor(x => x.Document!)
                .MaximumLength(20).WithMessage("O documento não pode ultrapassar 20 caracteres.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Notes), () =>
        {
            RuleFor(x => x.Notes!)
                .MaximumLength(500).WithMessage("As observações não podem ultrapassar 500 caracteres.");
        });
    }
}
