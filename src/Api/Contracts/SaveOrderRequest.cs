
namespace Api.Contracts;

public sealed record SaveOrderRequest(
    string? CustomerId,
    List<SaveOrderLineRequest>? Lines)
{
  public bool TryValidate(out Dictionary<string, string[]> errors)
  {
    var found = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(CustomerId) || CustomerId.Length > 50)
      found[nameof(CustomerId)] = ["ID покупця обов'язковий."];


    if (Lines is null || Lines.Count == 0)
      found[nameof(Lines)] = ["Замовлення повинно містити хоча б один товар."];

    else
    {
      for (int i = 0; i < Lines.Count; i++)
      {
        var line = Lines[i];

        if (line is null)
        {
          found[$"Lines[{i}]"] = ["Товарна позиція не може бути null."];
          continue;
        }

        if (string.IsNullOrWhiteSpace(line.ProductId) || line.ProductId.Length > 50)

          found[$"Lines[{i}].ProductId"] = ["ID товару обов'язковий."];

        if (string.IsNullOrWhiteSpace(line.Name) || line.Name.Length > 200)
          found[$"Lines[{i}].Name"] = ["Назва товару обов'язкова."];

        if (line.Price < 0)
          found[$"Lines[{i}].Price"] = ["Ціна не може бути від'ємною."];

        if (line.Quantity <= 0)
          found[$"Lines[{i}].Quantity"] = ["Кількість повинна бути більшою за нуль."];

      }
    }

    errors = found;
    return found.Count == 0;
  }
}

public sealed record SaveOrderLineRequest(
    string? ProductId,
    string? Name,
    decimal Price,
    int Quantity
);
