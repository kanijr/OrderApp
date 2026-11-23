
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
using Microsoft.Data.Sqlite;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

var rawConnectionString =
    builder.Configuration.GetConnectionString("AppDb")
    ?? "Data Source=data/app.db";

var csb = new SqliteConnectionStringBuilder(rawConnectionString);

if (!string.IsNullOrWhiteSpace(csb.DataSource) &&
    csb.DataSource != ":memory:")
{
  string databasePath = csb.DataSource;

  if (!Path.IsPathRooted(databasePath))
  {
    databasePath = Path.GetFullPath(
        Path.Combine(
            builder.Environment.ContentRootPath,
            databasePath));
  }

  string? directory = Path.GetDirectoryName(databasePath);

  if (!string.IsNullOrEmpty(directory))
    Directory.CreateDirectory(directory);

  csb.DataSource = databasePath;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(csb.ConnectionString));

builder.Services.AddScoped<IAsyncOrderStore, EfOrderStore>();

builder.Services.AddTransient<AsyncOrderService>();

builder.Services.AddScoped<IValidator<SaveOrderRequest>,
    SaveOrderRequestValidator>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
  using var scope = app.Services.CreateScope();

  var db = scope.ServiceProvider
      .GetRequiredService<AppDbContext>();

  db.Database.Migrate();

  app.MapOpenApi();
  app.MapScalarApiReference();
}

app.MapOrders();

app.Run();

public partial class Program { }