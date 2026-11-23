
using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class SaveOrderRequestValidator
    : AbstractValidator<SaveOrderRequest>
{
  public SaveOrderRequestValidator()
  {
    RuleFor(x => x.CustomerId)
        .NotEmpty()
            .WithMessage("ID покупця обов'язковий.");

    RuleFor(x => x.Lines)
        .NotNull()
            .WithMessage("Список товарів обов'язковий.")
        .Must(lines => lines is { Count: > 0 })
            .WithMessage("Замовлення повинно містити хоча б один товар.");

    RuleForEach(x => x.Lines)
        .NotNull()
            .WithMessage("Товарна позиція не може бути null.")
        .SetValidator(new SaveOrderLineRequestValidator());
  }
}
