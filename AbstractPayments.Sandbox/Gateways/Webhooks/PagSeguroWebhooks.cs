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
/// Validates incoming webhook signature authenticity for PagSeguro.
/// </summary>
public class PagSeguroSignatureValidator : IWebhookSignatureValidator
{
    /// <inheritdoc />
    public Task<bool> ValidateAsync(WebhookContext context)
    {
        if (context.Headers.TryGetValue("X-Signature", out var signature) && signature == "pagseguro_secret")
        {
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}

/// <summary>
/// Converts raw PagSeguro webhook payloads to normalized WebhookEvent domain models.
/// </summary>
public class PagSeguroEventConverter : IWebhookEventConverter
{
    /// <inheritdoc />
    public Task<WebhookEvent> ConvertAsync(WebhookContext context)
    {
        using var doc = JsonDocument.Parse(context.Body);
        var root = doc.RootElement;

        string eventId = "ps_evt_" + Guid.NewGuid().ToString("N")[..12];
        string txId = string.Empty;

        if (root.TryGetProperty("reference_id", out var refProp))
        {
            txId = refProp.GetString() ?? string.Empty;
        }

        return Task.FromResult(new WebhookEvent(
            eventId: eventId,
            provider: "pagseguro",
            receivedAt: DateTime.UtcNow,
            payload: txId
        ));
    }
}

/// <summary>
/// Handles normalized WebhookEvents for PagSeguro.
/// </summary>
public class PagSeguroEventHandler : IWebhookEventHandler
{
    private readonly ITransactionRepository _repository;
    private readonly ILogger<PagSeguroEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PagSeguroEventHandler"/> class.
    /// </summary>
    public PagSeguroEventHandler(ITransactionRepository repository, ILogger<PagSeguroEventHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(WebhookEvent @event)
    {
        _logger.LogInformation("Processing PagSeguro webhook event {EventId}", @event.EventId);
        string txId = @event.Payload;

        var tx = await _repository.GetByIdAsync(txId);
        if (tx != null)
        {
            tx.Status = ETransactionStatus.Paid;
            await _repository.UpdateAsync(tx);
            _logger.LogInformation("PagSeguro payment {TransactionId} updated to Paid", txId);
        }
    }
}
