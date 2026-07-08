namespace AbstractPayments.Core.Models.Webhooks;

using System;

/// <summary>
/// Unified standardized domain model representing a normalized gateway webhook payment event.
/// </summary>
public class WebhookEvent
{
    /// <summary>
    /// Gets the unique identifier of this specific event from the provider.
    /// </summary>
    public string EventId { get; }

    /// <summary>
    /// Gets the provider name (e.g. "mercadopago").
    /// </summary>
    public string Provider { get; }

    /// <summary>
    /// Gets the timestamp when the event was received/generated.
    /// </summary>
    public DateTime ReceivedAt { get; }

    /// <summary>
    /// Gets the raw event payload string used by handlers for provider-specific transformations.
    /// </summary>
    public string Payload { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookEvent"/> class.
    /// </summary>
    /// <param name="eventId">The unique provider event identifier.</param>
    /// <param name="provider">The gateway provider name.</param>
    /// <param name="receivedAt">The timestamp when the event was received.</param>
    /// <param name="payload">The raw provider event payload.</param>
    public WebhookEvent(
        string eventId,
        string provider,
        DateTime receivedAt,
        string payload)
    {
        EventId = eventId;
        Provider = provider;
        ReceivedAt = receivedAt;
        Payload = payload;
    }
}
