
using Api.Endpoints;
using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<IAsyncOrderStore>(_ =>
    new AsyncInMemoryOrderStore(SampleData.Orders()));

builder.Services.AddTransient<AsyncOrderService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.MapScalarApiReference();
}

app.MapOrders();

app.Run();
