
using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class ValidationFilter : IEndpointFilter
{
  public async ValueTask<object?> InvokeAsync(
      EndpointFilterInvocationContext context,
      EndpointFilterDelegate next)
  {
    var request = context.Arguments
        .OfType<SaveOrderRequest>()
        .FirstOrDefault();

    if (request is null)
      return await next(context);

    var validator = context.HttpContext
        .RequestServices
        .GetRequiredService<IValidator<SaveOrderRequest>>();

    var result = await validator.ValidateAsync(
        request,
        context.HttpContext.RequestAborted);

    if (!result.IsValid)
    {
      var errors = result.Errors
          .GroupBy(e => e.PropertyName)
          .ToDictionary(
              g => g.Key,
              g => g.Select(e => e.ErrorMessage).ToArray());

      return TypedResults.ValidationProblem(errors);
    }

    return await next(context);
  }
}
