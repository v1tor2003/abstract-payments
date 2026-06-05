using System;
using System.Reflection;
using AbstractPayments.Core.Abstractions;
using AbstractPayments.Core.Extensions;
using AbstractPayments.Core.Extensions.Payments;
using AbstractPayments.Core.Extensions.Webhooks;
using AbstractPayments.Sandbox.Coupled;
using AbstractPayments.Sandbox.Endpoints;
using AbstractPayments.Sandbox.Gateways;
using AbstractPayments.Sandbox.Storage;
using AbstractPayments.Sandbox.Gateways.Webhooks;
using AbstractPayments.Sandbox.Http;
using AbstractPayments.Sandbox.Requests;
using AbstractPayments.Sandbox.Responses;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Sandbox.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Register Storage & Infrastructure Dependencies
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Register Webhook Queue & Background Worker
builder.Services.AddSingleton<InMemoryWebhookQueue>();
builder.Services.AddSingleton<IWebhookQueue>(sp => sp.GetRequiredService<InMemoryWebhookQueue>());
builder.Services.AddHostedService<WebhookQueueProcessor>();

// Register Coupled Clients
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.gateway-sandbox.com");
});

builder.Services.AddScoped<MercadoPagoDirectClient>();
builder.Services.AddScoped<PagSeguroDirectClient>();
builder.Services.AddScoped<EfiBankDirectClient>();

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
        payment.AddProvider<IPixGateway, MercadoPagoPixGateway>("mercadopago");
        payment.AddProvider<IPixGateway, PagSeguroPixGateway>("pagseguro");
        payment.AddProvider<IPixGateway, EfiBankPixGateway>("efibank");
    })
    .AddEventsModule(events =>
    {
        events.IngestionEndpoint = "/v1/api/payments/webhook";

        events.ListenFrom("mercadopago", options => options
            .UseSignatureValidator<MercadoPagoSignatureValidator>()
            .UseConverter<MercadoPagoEventConverter>()
            .UseHandler<MercadoPagoEventHandler>());

        events.ListenFrom("pagseguro", options => options
            .UseSignatureValidator<PagSeguroSignatureValidator>()
            .UseConverter<PagSeguroEventConverter>()
            .UseHandler<PagSeguroEventHandler>());

        events.ListenFrom("efibank", options => options
            .UseSignatureValidator<EfiBankSignatureValidator>()
            .UseConverter<EfiBankEventConverter>()
            .UseHandler<EfiBankEventHandler>());
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