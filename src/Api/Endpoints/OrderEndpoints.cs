
using Api.Contracts;
using Api.Validation;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class OrderEndpoints
{
  public static void MapOrders(this WebApplication app)
  {
    var group = app.MapGroup("/api/orders").WithTags("Orders");

    group.MapGet("", async Task<Results<
          Ok<PagedResult<OrderDto>>,
          ValidationProblem>> (
            int? page,
            int? pageSize,
            string? search,
            IAsyncOrderStore store,
            CancellationToken ct) =>
        {
          int currentPage = page ?? 1;
          int currentPageSize = pageSize ?? 20;

          var errors = new Dictionary<string, string[]>();

          if (currentPage < 1)
            errors[nameof(page)] = ["Номер сторінки повинен бути більшим за нуль."];

          if (currentPageSize < 1 || currentPageSize > 100)
            errors[nameof(pageSize)] = ["Розмір сторінки повинен бути від 1 до 100."];

          if (errors.Count > 0)
            return TypedResults.ValidationProblem(errors);

          var orders = await store.ListAsync(ct);

          IEnumerable<Order> query = orders;

          if (!string.IsNullOrWhiteSpace(search))
          {
            string term = search.Trim();

            query = query.Where(order =>
              order.Id.Contains(term, StringComparison.OrdinalIgnoreCase) ||
              order.CustomerId.Contains(term, StringComparison.OrdinalIgnoreCase) ||
              order.Lines.Any(line =>
                  line.Name.Contains(term, StringComparison.OrdinalIgnoreCase)));
          }

          int total = query.Count();

          var items = query.OrderBy(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .Skip(((long)currentPage - 1) * currentPageSize > int.MaxValue
                ? int.MaxValue
                : (int)((long)currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .Select(OrderDto.FromDomain)
            .ToList();

          return TypedResults.Ok(new PagedResult<OrderDto>(items, total));
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

    group.MapPost("", async Task<
          Results<Created<OrderDto>, ValidationProblem>> (
            SaveOrderRequest request,
            AsyncOrderService service,
            CancellationToken ct) =>
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
        })
        .AddEndpointFilter<ValidationFilter>()
        .ProducesProblem(StatusCodes.Status409Conflict);

    group.MapPut("/{id}", async Task<
          Results<Ok<OrderDto>, NotFound, ValidationProblem>> (
            string id,
            SaveOrderRequest request,
            IAsyncOrderStore store,
            CancellationToken ct) =>
        {
          var existing = await store.GetByIdAsync(id, ct);

          if (existing is null)
            return TypedResults.NotFound();

          if (existing.Status != OrderStatus.Draft)
            throw new DomainException("Можна редагувати лише чернетку замовлення.");

          var customerId = request.CustomerId!.Trim();

          if (!string.Equals(existing.CustomerId, customerId, StringComparison.Ordinal))
            throw new DomainException(
              "Не можна змінювати покупця для існуючого замовлення.");

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
        })
        .AddEndpointFilter<ValidationFilter>()
        .ProducesProblem(StatusCodes.Status409Conflict);

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

    group.MapPost("/{id}/confirm", async Task<
          Results<Ok<OrderDto>, NotFound>> (
            string id,
            AsyncOrderService service,
            CancellationToken ct) =>
        {
          var updated = await service.ConfirmOrderAsync(id, ct);

          if (updated is null)
            return TypedResults.NotFound();

          return TypedResults.Ok(
              OrderDto.FromDomain(updated));
        })
        .ProducesProblem(StatusCodes.Status409Conflict);

    group.MapPost("/{id}/cancel", async Task<
          Results<Ok<OrderDto>, NotFound>> (
            string id,
            AsyncOrderService service,
            CancellationToken ct) =>
        {
          var updated = await service.CancelOrderAsync(id, ct);

          if (updated is null)
            return TypedResults.NotFound();

          return TypedResults.Ok(
              OrderDto.FromDomain(updated));
        })
        .ProducesProblem(StatusCodes.Status409Conflict);
  }
}
