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
/// Validates incoming webhook signature authenticity for EfiBank.
/// </summary>
public class EfiBankSignatureValidator : IWebhookSignatureValidator
{
    /// <inheritdoc />
    public Task<bool> ValidateAsync(WebhookContext context)
    {
        if (context.Headers.TryGetValue("X-Signature", out var signature) && signature == "efibank_secret")
        {
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}

/// <summary>
/// Converts raw EfiBank webhook payloads to normalized WebhookEvent domain models.
/// </summary>
public class EfiBankEventConverter : IWebhookEventConverter
{
    /// <inheritdoc />
    public Task<WebhookEvent> ConvertAsync(WebhookContext context)
    {
        using var doc = JsonDocument.Parse(context.Body);
        var root = doc.RootElement;

        string eventId = "efi_evt_" + Guid.NewGuid().ToString("N")[..12];
        string txId = string.Empty;
        string e2eId = string.Empty;

        if (root.TryGetProperty("pix", out var pixArray) && pixArray.ValueKind == JsonValueKind.Array && pixArray.GetArrayLength() > 0)
        {
            var item = pixArray[0];
            if (item.TryGetProperty("txid", out var txIdProp)) txId = txIdProp.GetString() ?? string.Empty;
            if (item.TryGetProperty("endToEndId", out var e2eProp)) e2eId = e2eProp.GetString() ?? string.Empty;
        }

        var normalizedPayload = JsonSerializer.Serialize(new { txid = txId, endToEndId = e2eId });

        return Task.FromResult(new WebhookEvent(
            eventId: eventId,
            provider: "efibank",
            receivedAt: DateTime.UtcNow,
            payload: normalizedPayload
        ));
    }
}

/// <summary>
/// Handles normalized WebhookEvents for EfiBank.
/// </summary>
public class EfiBankEventHandler : IWebhookEventHandler
{
    private readonly ITransactionRepository _repository;
    private readonly ILogger<EfiBankEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfiBankEventHandler"/> class.
    /// </summary>
    public EfiBankEventHandler(ITransactionRepository repository, ILogger<EfiBankEventHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(WebhookEvent @event)
    {
        _logger.LogInformation("Processing EfiBank webhook event {EventId}", @event.EventId);
        
        using var doc = JsonDocument.Parse(@event.Payload);
        var root = doc.RootElement;

        if (root.TryGetProperty("txid", out var txProp) && root.TryGetProperty("endToEndId", out var e2eProp))
        {
            string txId = txProp.GetString()!;
            string e2eId = e2eProp.GetString()!;

            var tx = await _repository.GetByIdAsync(txId);
            if (tx != null)
            {
                tx.Status = ETransactionStatus.Paid;
                tx.EndToEndId = e2eId;
                await _repository.UpdateAsync(tx);
                _logger.LogInformation("EfiBank payment {TransactionId} updated to Paid with EndToEndId {EndToEndId}", txId, e2eId);
            }
        }
    }
}
