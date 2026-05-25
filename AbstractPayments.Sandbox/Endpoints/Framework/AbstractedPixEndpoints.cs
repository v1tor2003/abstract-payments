namespace AbstractPayments.Sandbox.Endpoints.Framework;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions;
using AbstractPayments.Core.Exceptions;
using AbstractPayments.Sandbox.Diagnostics;
using AbstractPayments.Sandbox.Storage;
using AbstractPayments.Sandbox.Storage.Models;
using System.IO;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Core.Models.Webhooks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

/// <summary>
/// Vertical slice mapping the framework-decoupled Pix payment routes.
/// </summary>
public class AbstractedPixEndpoints : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/api/payments/pix")
            .WithTags("Framework Payments (Decoupled Approach)");

        group.MapPost("/", CreateAbstractedPixPaymentAsync)
            .WithSummary("Create Pix Payment (Framework)")
            .WithDescription("Initiates a Pix payment transaction using the AbstractPayments framework. Completely decoupled from specific gateway implementations.")
            .Produces<AbstractedPixResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetAbstractedPixPaymentsAsync)
            .WithSummary("List Pix Payments (Framework)")
            .WithDescription("Retrieves all Pix payment transactions registered via the decoupled framework approach.")
            .Produces<IEnumerable<Transaction>>(StatusCodes.Status200OK);

        group.MapGet("/{transactionId}", GetAbstractedPixPaymentByIdAsync)
            .WithSummary("Get Pix Payment by ID (Framework)")
            .WithDescription("Fetches a single decoupled Pix payment transaction by its identifier.")
            .Produces<Transaction>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/v1/api/payments/webhook", HandleAbstractedWebhookAsync)
            .WithTags("Framework Payments (Decoupled Approach)")
            .WithSummary("Handle Webhook Notification (Framework)")
            .WithDescription("Processes a payment webhook notification. Completely delegated to the core IWebhookProcessor orchestration engine.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAbstractedWebhookAsync(
        HttpRequest httpRequest,
        [FromServices] IWebhookProcessor processor,
        [FromServices] ILogger<AbstractedPixEndpoints> logger)
    {
        using var reader = new StreamReader(httpRequest.Body);
        var rawBody = await reader.ReadToEndAsync();

        var headers = new Dictionary<string, string>();
        foreach (var header in httpRequest.Headers)
        {
            headers[header.Key] = header.Value.ToString();
        }

        var context = new WebhookContext("fake", rawBody, headers);

        await processor.ProcessAsync(context);

        return TypedResults.NoContent();
    }

    private static async Task<Ok<AbstractedPixResponse>> CreateAbstractedPixPaymentAsync(
        [FromBody] AbstractedPixRequest request,
        [FromServices] IPaymentGatewayFactory gatewayFactory,
        [FromServices] ITransactionRepository repository,
        [FromServices] ILogger<AbstractedPixEndpoints> logger)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("The payment amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.Provider))
        {
            throw new ArgumentException("Provider name must be explicitly declared.");
        }

        logger.LogPaymentInitiated(request.Amount, request.Provider);

        var gateway = gatewayFactory.Get<IPixGateway>(request.Provider);
        var paymentString = await gateway.GeneratePaymentAsync();

        var transaction = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            Amount = request.Amount,
            Provider = request.Provider,
            PaymentString = paymentString,
            Status = ETransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await repository.InsertAsync(transaction);

        logger.LogPaymentGenerated(transaction.Id, transaction.PaymentString);

        return TypedResults.Ok(new AbstractedPixResponse(
            transaction.Id,
            transaction.Amount,
            transaction.Provider,
            transaction.PaymentString,
            transaction.Status.ToString(),
            transaction.CreatedAt
        ));
    }

    private static async Task<Ok<IEnumerable<Transaction>>> GetAbstractedPixPaymentsAsync(
        [FromServices] ITransactionRepository repository)
    {
        var list = await repository.GetAllAsync();
        return TypedResults.Ok(list);
    }

    private static async Task<Results<Ok<Transaction>, NotFound>> GetAbstractedPixPaymentByIdAsync(
        string transactionId,
        [FromServices] ITransactionRepository repository)
    {
        var tx = await repository.GetByIdAsync(transactionId);
        if (tx == null)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(tx);
    }
}
