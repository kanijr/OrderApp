
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Api.Tests;

public sealed class OrderEndpointsTests
{
  [Fact]
  public async Task Post_ThenGetByLocation_ReturnsCreatedOrder()
  {
    using var factory = new OrderApiFactory();
    using var client = factory.CreateClient();

    var request = new
    {
      customerId = "C-TEST-001",
      lines = new[]
        {
          new
          {
              productId = "P-001",
              name = "Ноутбук Lenovo",
              price = 29999.99m,
              quantity = 1
          }
        }
    };

    var postResponse = await client.PostAsJsonAsync("/api/orders", request);
    Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

    var location = postResponse.Headers.Location;
    Assert.NotNull(location);

    var getResponse = await client.GetAsync(location);
    Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

    var json = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

    Assert.Equal("C-TEST-001", json.GetProperty("customerId").GetString());
    Assert.Equal("Draft", json.GetProperty("status").GetString());
    Assert.Equal(29999.99m, json.GetProperty("total").GetDecimal());
    Assert.Single(json.GetProperty("lines").EnumerateArray());
  }

  [Fact]
  public async Task Post_InvalidOrder_ReturnsValidationProblem()
  {
    using var factory = new OrderApiFactory();
    using var client = factory.CreateClient();

    var request = new
    {
      customerId = "",
      lines = Array.Empty<object>()
    };

    var response = await client.PostAsJsonAsync("/api/orders", request);
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    var json = await response.Content.ReadFromJsonAsync<JsonElement>();
    Assert.Equal(400, json.GetProperty("status").GetInt32());

    var errors = json.GetProperty("errors");
    Assert.True(errors.TryGetProperty("CustomerId", out _));
    Assert.True(errors.TryGetProperty("Lines", out _));
  }

  [Fact]
  public async Task Confirm_Twice_ReturnsConflict()
  {
    using var factory = new OrderApiFactory();
    using var client = factory.CreateClient();

    var request = new
    {
      customerId = "C-TEST-002",
      lines = new[]
        {
          new
          {
              productId = "P-002",
              name = "Миша Logitech",
              price = 799.50m,
              quantity = 1
          }
        }
    };

    var created = await client.PostAsJsonAsync("/api/orders", request);
    Assert.Equal(HttpStatusCode.Created, created.StatusCode);

    var location = created.Headers.Location;
    Assert.NotNull(location);

    var confirmUrl = $"{location.ToString().TrimEnd('/')}/confirm";

    var first = await client.PostAsync(confirmUrl, null);
    Assert.Equal(HttpStatusCode.OK, first.StatusCode);

    var second = await client.PostAsync(confirmUrl, null);
    Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

    var json = await second.Content.ReadFromJsonAsync<JsonElement>();
    Assert.Equal(409, json.GetProperty("status").GetInt32());
    Assert.Equal("Порушено бізнес-правило замовлення",
      json.GetProperty("title").GetString());
  }
}
