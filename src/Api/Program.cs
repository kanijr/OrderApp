
using Api.Contracts;
using Api.Endpoints;
using Api.Validation;
using Api.Exceptions;
using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;
using FluentValidation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddSingleton<IAsyncOrderStore>(_ =>
    new AsyncInMemoryOrderStore(SampleData.Orders()));

builder.Services.AddTransient<AsyncOrderService>();

builder.Services.AddScoped<IValidator<SaveOrderRequest>,
    SaveOrderRequestValidator>();

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

public partial class Program { }