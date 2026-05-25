using System;
using System.Reflection;
using AbstractPayments.Core.Abstractions;
using AbstractPayments.Core.Extensions;
using AbstractPayments.Core.Extensions.Payments;
using AbstractPayments.Sandbox.Coupled;
using AbstractPayments.Sandbox.Endpoints;
using AbstractPayments.Sandbox.Gateways;
using AbstractPayments.Sandbox.Storage;
using AbstractPayments.Sandbox.Gateways.Webhooks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Register Storage & Infrastructure Dependencies
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Register Coupled Fake Client
builder.Services.AddSingleton<FakeDirectClient>();

// Register Authentication & Authorization services as required by middleware sequence standards
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Register Global Exception Handling & RFC 7807 Problem Details
builder.Services.AddExceptionHandler<AbstractPayments.Sandbox.Diagnostics.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Register OpenAPI spec generation services
builder.Services.AddOpenApi();

// Register AbstractPayments Core Framework & Module Gateway Capabilities
builder.Services.AddAbstractPayments()
    .AddPaymentsModule(payment =>
    {
        payment.AddProvider<IPixGateway, FakePixGateway>("fake");
    })
    .AddEventsModule(events =>
    {
        events.Endpoint = "/v1/api/payments/webhook";
        events.SignatureValidators.UseStrategy<FakeSignatureValidator>("fake");
        events.Converters.AddConverter<FakeEventConverter>("fake");
        events.Handlers.AddHandler<FakeEventHandler>("fake");
    });

// Register Assembly-Scanned IEndpoint implementations
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();

// Ensure SQLite Database & Table schemas exist on application startup
using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
    await repo.InitializeAsync();
}

// Configure the ASP.NET Core Middleware Pipeline in strict chronological sequence:
app.UseExceptionHandler();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map all assembly-scanned endpoints dynamically
app.MapEndpoints();

// Expose OpenAPI spec and interactive Scalar UI docs
app.MapOpenApi();
app.MapScalarApiReference();

app.Run();

// Expose Program class for WebApplicationFactory in integration tests
public partial class Program { }

// DTO Requests and Responses (shared across endpoints)
public record CoupledPixRequest(decimal Amount);
public record AbstractedPixRequest(decimal Amount, string Provider);
public record AbstractedPixResponse(
    string TransactionId,
    decimal Amount,
    string Provider,
    string PaymentString,
    string Status,
    DateTime CreatedAt
);
