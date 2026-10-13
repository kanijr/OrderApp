
namespace Core.Domain;

public sealed class Customer
{
  public string Id { get; }
  public string Name { get; }
  public string? Email { get; }

  private Customer(string id, string name, string? email)
  {
    Id = id;
    Name = name;
    Email = email;
  }

  public static Customer Create(string id, string name, string? email = null)
  {
    if (string.IsNullOrWhiteSpace(id))
      throw new ArgumentException(
          "Ідентифікатор покупця обов'язковий", nameof(id));

    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException(
          "Ім'я покупця обов'язкове", nameof(name));

    return new Customer(id.Trim(), name.Trim(),
      string.IsNullOrWhiteSpace(email) ? null : email.Trim());
  }
}
