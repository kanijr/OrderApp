
using Core.Abstractions;
using Core.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.Tests;

public sealed class OrderApiFactory
    : WebApplicationFactory<Program>
{
  protected override void ConfigureWebHost(
      IWebHostBuilder builder)
  {
    builder.UseEnvironment("Development");

    builder.ConfigureTestServices(services =>
    {
      services.RemoveAll<IAsyncOrderStore>();

      services.AddSingleton<IAsyncOrderStore>(
              new AsyncInMemoryOrderStore());
    });
  }
}
