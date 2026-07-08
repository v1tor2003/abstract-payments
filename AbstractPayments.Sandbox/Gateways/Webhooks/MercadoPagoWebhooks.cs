using System;
using System.Text.Json;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Core.Models.Webhooks;
using AbstractPayments.Sandbox.Storage;
using AbstractPayments.Sandbox.Storage.Models;
using Microsoft.Extensions.Logging;

namespace AbstractPayments.Sandbox.Gateways.Webhooks;

/// <summary>
/// Validates incoming webhook signature authenticity for Mercado Pago.
/// </summary>
public class MercadoPagoSignatureValidator : IWebhookSignatureValidator
{
    /// <inheritdoc />
    public Task<bool> ValidateAsync(WebhookContext context)
    {
        if (context.Headers.TryGetValue("X-Signature", out var signature) && signature == "mercadopago_secret")
        {
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}

/// <summary>
/// Converts raw Mercado Pago webhook payloads to normalized WebhookEvent domain models.
/// </summary>
public class MercadoPagoEventConverter : IWebhookEventConverter
{
    /// <inheritdoc />
    public Task<WebhookEvent> ConvertAsync(WebhookContext context)
    {
        using var doc = JsonDocument.Parse(context.Body);
        var root = doc.RootElement;

        string eventId = "mp_evt_" + Guid.NewGuid().ToString("N")[..12];
        string txId = string.Empty;

        if (root.TryGetProperty("data", out var dataProp) && dataProp.TryGetProperty("id", out var idProp))
        {
            txId = idProp.GetString() ?? string.Empty;
        }

        return Task.FromResult(new WebhookEvent(
            eventId: eventId,
            provider: "mercadopago",
            receivedAt: DateTime.UtcNow,
            payload: txId
        ));
    }
}

/// <summary>
/// Handles normalized WebhookEvents for Mercado Pago.
/// </summary>
public class MercadoPagoEventHandler : IWebhookEventHandler
{
    private readonly ITransactionRepository _repository;
    private readonly ILogger<MercadoPagoEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MercadoPagoEventHandler"/> class.
    /// </summary>
    public MercadoPagoEventHandler(ITransactionRepository repository, ILogger<MercadoPagoEventHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(WebhookEvent @event)
    {
        _logger.LogInformation("Processing Mercado Pago webhook event {EventId}", @event.EventId);
        string txId = @event.Payload;

        var tx = await _repository.GetByIdAsync(txId);
        if (tx != null)
        {
            tx.Status = ETransactionStatus.Paid;
            await _repository.UpdateAsync(tx);
            _logger.LogInformation("Mercado Pago payment {TransactionId} updated to Paid", txId);
        }
    }
}
