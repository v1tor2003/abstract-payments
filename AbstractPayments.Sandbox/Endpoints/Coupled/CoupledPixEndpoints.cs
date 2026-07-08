namespace AbstractPayments.Sandbox.Endpoints.Coupled;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AbstractPayments.Sandbox.Coupled;
using AbstractPayments.Sandbox.Diagnostics;
using AbstractPayments.Sandbox.Storage;
using AbstractPayments.Sandbox.Storage.Models;
using AbstractPayments.Sandbox.Http.Commands;
using AbstractPayments.Sandbox.Requests;
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
            .WithDescription("Initiates a Pix payment transaction by calling the specific Gateway directly without using the AbstractPayments framework.")
            .Produces<object>(StatusCodes.Status200OK)
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

        app.MapPost("/v1/api/coupled/payments/webhook/{provider}", HandleCoupledWebhookAsync)
            .WithTags("Coupled Payments (Legacy / Coupled Approach)")
            .WithSummary("Handle Webhook Notification (Coupled)")
            .WithDescription("Processes a payment update webhook notification using custom bespoke signature verification per provider.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleCoupledWebhookAsync(
        string provider,
        HttpRequest httpRequest,
        [FromServices] ITransactionRepository repository,
        [FromServices] ILogger<CoupledPixEndpoints> logger)
    {
        using var reader = new StreamReader(httpRequest.Body);
        var rawBody = await reader.ReadToEndAsync();
        
        using var doc = JsonDocument.Parse(rawBody);
        var root = doc.RootElement;
        
        string? txId = null;
        string? statusStr = null;
        string? e2eId = null;

        switch (provider.ToLowerInvariant())
        {
            case "mercadopago":
                if (!httpRequest.Headers.TryGetValue("X-Signature", out var mpSig) || mpSig != "mercadopago_coupled_secret")
                {
                    return TypedResults.Unauthorized();
                }
                if (root.TryGetProperty("data", out var mpData) && mpData.TryGetProperty("id", out var mpIdProp))
                {
                    txId = mpIdProp.GetString();
                }
                if (root.TryGetProperty("status", out var mpStatusProp))
                {
                    statusStr = mpStatusProp.GetString();
                }
                break;

            case "pagseguro":
                if (!httpRequest.Headers.TryGetValue("X-Signature", out var psSig) || psSig != "pagseguro_coupled_secret")
                {
                    return TypedResults.Unauthorized();
                }
                if (root.TryGetProperty("reference_id", out var psRef))
                {
                    txId = psRef.GetString();
                }
                if (root.TryGetProperty("status", out var psStatus))
                {
                    statusStr = psStatus.GetString();
                }
                break;

            case "efibank":
                if (!httpRequest.Headers.TryGetValue("X-Signature", out var efiSig) || efiSig != "efibank_coupled_secret")
                {
                    return TypedResults.Unauthorized();
                }
                if (root.TryGetProperty("pix", out var pixArray) && pixArray.ValueKind == JsonValueKind.Array && pixArray.GetArrayLength() > 0)
                {
                    var item = pixArray[0];
                    if (item.TryGetProperty("txid", out var efiTxProp)) txId = efiTxProp.GetString();
                    if (item.TryGetProperty("endToEndId", out var efiE2EProp)) e2eId = efiE2EProp.GetString();
                    statusStr = "Paid"; // BACEN notification is an assertion of payment
                }
                break;

            default:
                return TypedResults.BadRequest();
        }

        if (!string.IsNullOrEmpty(txId))
        {
            var tx = await repository.GetByIdAsync(txId);
            if (tx != null)
            {
                if (!string.IsNullOrEmpty(statusStr))
                {
                    if (statusStr.Equals("approved", StringComparison.OrdinalIgnoreCase) || 
                        statusStr.Equals("PAID", StringComparison.OrdinalIgnoreCase) || 
                        statusStr.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                    {
                        tx.Status = ETransactionStatus.Paid;
                    }
                }
                if (!string.IsNullOrEmpty(e2eId))
                {
                    tx.EndToEndId = e2eId;
                }
                await repository.UpdateAsync(tx);
                logger.LogInformation("Successfully updated transaction {TransactionId} to {Status} via coupled webhook.", txId, tx.Status);
                return TypedResults.NoContent();
            }
        }

        return TypedResults.BadRequest();
    }

    private static async Task<IResult> CreatePixPaymentAsync(
        [FromBody] CoupledPixRequest request,
        [FromServices] MercadoPagoDirectClient mpClient,
        [FromServices] PagSeguroDirectClient psClient,
        [FromServices] EfiBankDirectClient efiClient,
        [FromServices] ITransactionRepository repository,
        [FromServices] ILogger<CoupledPixEndpoints> logger)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("The payment amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.Provider))
        {
            throw new ArgumentException("Provider must be specified.");
        }

        string provider = request.Provider.ToLowerInvariant();
        logger.LogPaymentInitiated(request.Amount, provider);

        object result;
        string paymentString;

        switch (provider)
        {
            case "mercadopago":
                var mpResponse = await mpClient.CreatePixPaymentAsync(request.Amount);
                result = mpResponse;
                paymentString = mpResponse.PointOfInteraction.TransactionData.QrCode;
                break;

            case "pagseguro":
                var psResponse = await psClient.CreatePixPaymentAsync(request.Amount);
                result = psResponse;
                paymentString = psResponse.QrCodes[0].Text;
                break;

            case "efibank":
                var efiResponse = await efiClient.CreatePixPaymentAsync(request.Amount);
                result = efiResponse;
                paymentString = efiResponse.PixCopiaECola;
                break;

            default:
                throw new ArgumentException($"Unsupported provider: {provider}");
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            Amount = request.Amount,
            Provider = provider,
            PaymentString = paymentString,
            Status = ETransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await repository.InsertAsync(transaction);

        logger.LogPaymentGenerated(transaction.Id, transaction.PaymentString);

        return TypedResults.Ok(result);
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
