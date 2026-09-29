using FluentValidation;
using Komari.Application.Tables.DTOs;

namespace Komari.Application.Tables.Validators;

public class CreateTableValidator : AbstractValidator<CreateTableRequest>
{
    public CreateTableValidator()
    {
        RuleFor(x => x.Number)
            .GreaterThan(0).WithMessage("O número da mesa ou posição de balcão deve ser maior que zero.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("A capacidade de assentos deve ser de pelo menos 1 lugar.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("O tipo de atendimento informado é inválido.");

        RuleFor(x => x.Location)
            .MaximumLength(100).WithMessage("A localização/área não pode ultrapassar 100 caracteres.");
    }
}
