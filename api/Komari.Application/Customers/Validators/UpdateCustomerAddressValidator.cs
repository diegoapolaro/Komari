using FluentValidation;
using Komari.Application.Customers.DTOs;

namespace Komari.Application.Customers.Validators;

public class UpdateCustomerAddressValidator : AbstractValidator<UpdateCustomerAddressRequest>
{
    public UpdateCustomerAddressValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("O logradouro/rua é obrigatório.")
            .Length(2, 200).WithMessage("O logradouro deve ter entre 2 e 200 caracteres.");

        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("O número do endereço é obrigatório.")
            .MaximumLength(20).WithMessage("O número não pode exceder 20 caracteres.");

        RuleFor(x => x.Neighborhood)
            .NotEmpty().WithMessage("O bairro é obrigatório.")
            .Length(2, 100).WithMessage("O bairro deve ter entre 2 e 100 caracteres.");

        When(x => !string.IsNullOrWhiteSpace(x.ZipCode), () =>
        {
            RuleFor(x => x.ZipCode!)
                .MaximumLength(10).WithMessage("O CEP não pode exceder 10 caracteres.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Complement), () =>
        {
            RuleFor(x => x.Complement!)
                .MaximumLength(100).WithMessage("O complemento não pode exceder 100 caracteres.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.ReferencePoint), () =>
        {
            RuleFor(x => x.ReferencePoint!)
                .MaximumLength(200).WithMessage("O ponto de referência não pode exceder 200 caracteres.");
        });
    }
}
