
using Core;
using Microsoft.AspNetCore.Diagnostics;

namespace Api.Exceptions;

public sealed class DomainExceptionHandler : IExceptionHandler
{
  public async ValueTask<bool> TryHandleAsync(
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken)
  {
    if (exception is not DomainException domainException)
      return false;

    var result = TypedResults.Problem(
        title: "Порушено бізнес-правило замовлення",
        detail: domainException.Message,
        statusCode: StatusCodes.Status409Conflict
    );

    await result.ExecuteAsync(httpContext);

    return true;
  }
}
