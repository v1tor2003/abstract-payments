namespace AbstractPayments.Core.Extensions.Options;

using System;

/// <summary>
/// Options class specifically for webhook and event handling configurations.
/// </summary>
public class WebhookOptions
{
    /// <summary>
    /// Gets or sets the webhook ingestion HTTP route endpoint.
    /// </summary>
    public string IngestionEndpoint { get; set; } = "/v1/api/payments/webhook";

    /// <summary>
    /// Gets or sets the number of retry attempts for failed custom event handlers.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the allowed time skew window to prevent replay or spamming attacks.
    /// </summary>
    public TimeSpan TimeSkewWindow { get; set; } = TimeSpan.FromMinutes(5);
}
