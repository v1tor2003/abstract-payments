namespace AbstractPayments.Sandbox.Gateways.Webhooks;

using System;
using System.Text.Json;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Core.Models.Webhooks;

/// <summary>
/// Framework-compliant event converter translating raw BACEN Pix webhook payload into WebhookEvent domain model.
/// </summary>
public class FakeEventConverter : IWebhookEventConverter
{
    /// <inheritdoc />
    public Task<WebhookEvent> ConvertAsync(WebhookContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        try
        {
            using var doc = JsonDocument.Parse(context.Body);
            var root = doc.RootElement;

            if (root.TryGetProperty("pix", out var pixProp) && pixProp.ValueKind == JsonValueKind.Array)
            {
                var eventId = Guid.NewGuid().ToString();
                var payload = context.Body;

                var @event = new WebhookEvent(
                    eventId,
                    "fake",
                    DateTime.UtcNow,
                    payload
                );

                return Task.FromResult(@event);
            }
        }
        catch { }

        throw new ArgumentException("Invalid webhook payload structure");
    }
}
