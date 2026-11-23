
using Api.Contracts;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class OrderEndpoints
{
  public static void MapOrders(this WebApplication app)
  {
    var group = app.MapGroup("/api/orders").WithTags("Orders");

    group.MapGet("", async (
        IAsyncOrderStore store,
        CancellationToken ct) =>
    {
      var orders = await store.ListAsync(ct);

      return TypedResults.Ok(
          orders.Select(OrderDto.FromDomain).ToList());
    });

    group.MapGet("/{id}", async
          Task<Results<Ok<OrderDto>, NotFound>> (
            string id,
            IAsyncOrderStore store,
            CancellationToken ct) =>
        {
          var order = await store.GetByIdAsync(id, ct);

          if (order is null)
            return TypedResults.NotFound();

          return TypedResults.Ok(OrderDto.FromDomain(order));
        });

    group.MapPost("", async Task<Results<
          Created<OrderDto>,
          ValidationProblem,
          Conflict<ProblemDetails>>> (
            SaveOrderRequest request,
            AsyncOrderService service,
            CancellationToken ct) =>
        {
          if (!request.TryValidate(out var errors))
            return TypedResults.ValidationProblem(errors);

          try
          {
            var lines = request.Lines!
                .Select(line => new Core.Dto.OrderLineDto(
                    line.ProductId!.Trim(),
                    line.Name!.Trim(),
                    line.Price,
                    line.Quantity))
                .ToList();

            var created = await service.CreateOrderAsync(
                request.CustomerId!.Trim(),
                lines,
                ct);

            return TypedResults.Created($"/api/orders/{created.Id}",
                    OrderDto.FromDomain(created));
          }
          catch (DomainException ex)
          {
            return BusinessConflict(ex.Message);
          }
        });

    group.MapPut("/{id}", async Task<Results<
          Ok<OrderDto>,
          NotFound,
          ValidationProblem,
          Conflict<ProblemDetails>>> (
            string id,
            SaveOrderRequest request,
            IAsyncOrderStore store,
            CancellationToken ct) =>
        {
          if (!request.TryValidate(out var errors))
            return TypedResults.ValidationProblem(errors);

          var existing = await store.GetByIdAsync(id, ct);

          if (existing is null)
            return TypedResults.NotFound();

          if (existing.Status != OrderStatus.Draft)
            return BusinessConflict("Можна редагувати лише чернетку замовлення.");

          var customerId = request.CustomerId!.Trim();

          if (!string.Equals(existing.CustomerId, customerId, StringComparison.Ordinal))
            return
              BusinessConflict("Не можна змінювати покупця для існуючого замовлення.");

          try
          {
            var replacement = Order.CreateWithId(id, customerId, existing.CreatedAt);
            foreach (var line in request.Lines!)
            {
              ct.ThrowIfCancellationRequested();
              replacement.AddLine(
                  line.ProductId!.Trim(),
                  line.Name!.Trim(),
                  line.Price,
                  line.Quantity);
            }

            await store.UpdateAsync(replacement, ct);

            return TypedResults.Ok(OrderDto.FromDomain(replacement));
          }
          catch (DomainException ex)
          {
            return BusinessConflict(ex.Message);
          }
        });

    group.MapDelete("/{id}", async Task<
          Results<NoContent, NotFound>> (
            string id,
            IAsyncOrderStore store,
            CancellationToken ct) =>
        {
          bool removed = await store.RemoveAsync(id, ct);

          return removed
              ? TypedResults.NoContent()
              : TypedResults.NotFound();
        });

    group.MapGet("/{id}/total", async Task<
          Results<Ok<OrderTotalDto>, NotFound>> (
            string id,
            IAsyncOrderStore store,
            CancellationToken ct) =>
        {
          var order = await store.GetByIdAsync(id, ct);

          if (order is null)
            return TypedResults.NotFound();

          return TypedResults.Ok(
                  new OrderTotalDto(order.Id, order.Total));
        });

    group.MapPost("/{id}/confirm", async Task<Results<
          Ok<OrderDto>,
          NotFound,
          Conflict<ProblemDetails>>> (
            string id,
            AsyncOrderService service,
            CancellationToken ct) =>
        {
          try
          {
            var updated = await service.ConfirmOrderAsync(id, ct);

            if (updated is null)
              return TypedResults.NotFound();

            return TypedResults.Ok(
                OrderDto.FromDomain(updated));
          }
          catch (DomainException ex)
          {
            return BusinessConflict(ex.Message);
          }
        });

    group.MapPost("/{id}/cancel", async Task<Results<
          Ok<OrderDto>,
          NotFound,
          Conflict<ProblemDetails>>> (
            string id,
            AsyncOrderService service,
            CancellationToken ct) =>
        {
          try
          {
            var updated = await service.CancelOrderAsync(id, ct);

            if (updated is null)
              return TypedResults.NotFound();

            return TypedResults.Ok(
                OrderDto.FromDomain(updated));
          }
          catch (DomainException ex)
          {
            return BusinessConflict(ex.Message);
          }
        });
  }

  private static Conflict<ProblemDetails> BusinessConflict(
      string message)
  {
    return TypedResults.Conflict(new ProblemDetails
    {
      Title = "Порушено бізнес-правило замовлення",
      Detail = message,
      Status = StatusCodes.Status409Conflict
    });
  }
}
