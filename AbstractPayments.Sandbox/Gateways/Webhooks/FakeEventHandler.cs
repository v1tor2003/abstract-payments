namespace AbstractPayments.Sandbox.Gateways.Webhooks;

using System;
using System.Text.Json;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Core.Models.Webhooks;
using AbstractPayments.Sandbox.Storage;
using AbstractPayments.Sandbox.Storage.Models;
using Microsoft.Extensions.Logging;

/// <summary>
/// Framework-compliant event handler acting upon normalized Fake Gateway WebhookEvents.
/// </summary>
public class FakeEventHandler : IWebhookEventHandler
{
    private readonly ITransactionRepository _repository;
    private readonly ILogger<FakeEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeEventHandler"/> class.
    /// </summary>
    public FakeEventHandler(ITransactionRepository repository, ILogger<FakeEventHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(WebhookEvent @event)
    {
        _logger.LogInformation("Processing framework webhook event {EventId} for provider {Provider}", @event.EventId, @event.Provider);

        using var doc = JsonDocument.Parse(@event.Payload);
        var root = doc.RootElement;

        if (root.TryGetProperty("pix", out var pixProp) && pixProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in pixProp.EnumerateArray())
            {
                if (item.TryGetProperty("txid", out var txIdProp) &&
                    item.TryGetProperty("endToEndId", out var e2eProp))
                {
                    string txId = txIdProp.GetString()!;
                    string e2eId = e2eProp.GetString()!;

                    var tx = await _repository.GetByIdAsync(txId);
                    if (tx != null)
                    {
                        if (tx.Status == ETransactionStatus.Paid && tx.EndToEndId == e2eId)
                        {
                            _logger.LogInformation("Transaction {TransactionId} already processed with endToEndId {EndToEndId}. Skipping.", txId, e2eId);
                            continue;
                        }

                        tx.Status = ETransactionStatus.Paid;
                        tx.EndToEndId = e2eId;
                        await _repository.UpdateAsync(tx);
                        _logger.LogInformation("Successfully approved transaction {TransactionId} with endToEndId {EndToEndId} via framework webhook.", txId, e2eId);
                    }
                    else
                    {
                        _logger.LogWarning("Transaction {TransactionId} not found during webhook processing.", txId);
                    }
                }
            }
        }
    }
}
