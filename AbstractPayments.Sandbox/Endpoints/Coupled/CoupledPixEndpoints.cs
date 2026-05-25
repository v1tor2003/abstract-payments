namespace AbstractPayments.Sandbox.Endpoints.Coupled;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPayments.Sandbox.Coupled;
using AbstractPayments.Sandbox.Diagnostics;
using AbstractPayments.Sandbox.Storage;
using AbstractPayments.Sandbox.Storage.Models;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

/// <summary>
/// Vertical slice mapping the coupled Pix payment routes.
/// </summary>
public class CoupledPixEndpoints : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/api/coupled/payments/pix")
            .WithTags("Coupled Payments (Legacy / Coupled Approach)");

        group.MapPost("/", CreatePixPaymentAsync)
            .WithSummary("Create Pix Payment (Coupled)")
            .WithDescription("Initiates a Pix payment transaction by calling the Fake Gateway directly without using the AbstractPayments framework.")
            .Produces<FakePixResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetPixPaymentsAsync)
            .WithSummary("List Pix Payments (Coupled)")
            .WithDescription("Retrieves a list of all Pix payment transactions registered via the coupled approach.")
            .Produces<IEnumerable<Transaction>>(StatusCodes.Status200OK);

        group.MapGet("/{transactionId}", GetPixPaymentByIdAsync)
            .WithSummary("Get Pix Payment by ID (Coupled)")
            .WithDescription("Fetches a single Pix payment transaction by its unique identifier.")
            .Produces<Transaction>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/v1/api/coupled/payments/webhook", HandleCoupledWebhookAsync)
            .WithTags("Coupled Payments (Legacy / Coupled Approach)")
            .WithSummary("Handle Webhook Notification (Coupled)")
            .WithDescription("Processes a payment update webhook notification from MercadoPago using custom bespoke signature verification.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleCoupledWebhookAsync(
        HttpRequest httpRequest,
        [FromServices] ITransactionRepository repository,
        [FromServices] ILogger<CoupledPixEndpoints> logger)
    {
        if (!httpRequest.Headers.TryGetValue("X-Signature", out var signature) || signature != "fake_secret_signature")
        {
            return TypedResults.Unauthorized();
        }

        using var reader = new StreamReader(httpRequest.Body);
        var rawBody = await reader.ReadToEndAsync();
        
        using var doc = JsonDocument.Parse(rawBody);
        var root = doc.RootElement;

        if (root.TryGetProperty("transactionId", out var txProp) && 
            root.TryGetProperty("status", out var statusProp))
        {
            var txId = txProp.GetString()!;
            var status = statusProp.GetString()!;

            var tx = await repository.GetByIdAsync(txId);
            if (tx != null)
            {
                if (Enum.TryParse<ETransactionStatus>(status, true, out var parsedStatus))
                {
                    tx.Status = parsedStatus;
                }
                await repository.UpdateAsync(tx);
                logger.LogInformation("Successfully updated transaction {TransactionId} to {Status} via coupled webhook.", txId, status);
                return TypedResults.NoContent();
            }
        }

        return TypedResults.BadRequest();
    }

    private static async Task<Ok<FakePixResponse>> CreatePixPaymentAsync(
        [FromBody] CoupledPixRequest request,
        [FromServices] FakeDirectClient directClient,
        [FromServices] ITransactionRepository repository,
        [FromServices] ILogger<CoupledPixEndpoints> logger)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("The payment amount must be greater than zero.");
        }

        logger.LogPaymentInitiated(request.Amount, "fake_coupled");

        var response = await directClient.CreatePixPaymentAsync(request.Amount);

        var transaction = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            Amount = request.Amount,
            Provider = "fake_coupled",
            PaymentString = response.QrCodeBase64,
            Status = ETransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await repository.InsertAsync(transaction);

        logger.LogPaymentGenerated(transaction.Id, transaction.PaymentString);

        return TypedResults.Ok(response);
    }

    private static async Task<Ok<IEnumerable<Transaction>>> GetPixPaymentsAsync(
        [FromServices] ITransactionRepository repository)
    {
        var list = await repository.GetAllAsync();
        return TypedResults.Ok(list);
    }

    private static async Task<Results<Ok<Transaction>, NotFound>> GetPixPaymentByIdAsync(
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
