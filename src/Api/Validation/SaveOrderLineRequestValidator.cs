
using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class SaveOrderLineRequestValidator
    : AbstractValidator<SaveOrderLineRequest>
{
  public SaveOrderLineRequestValidator()
  {
    RuleFor(x => x.ProductId)
        .NotEmpty()
            .WithMessage("ID товару обов'язковий.");

    RuleFor(x => x.Name)
        .NotEmpty()
            .WithMessage("Назва товару обов'язкова.");

    RuleFor(x => x.Price)
        .GreaterThanOrEqualTo(0)
            .WithMessage("Ціна не може бути від'ємною.");

    RuleFor(x => x.Quantity)
        .GreaterThan(0)
            .WithMessage("Кількість повинна бути більшою за нуль.");
  }
}
