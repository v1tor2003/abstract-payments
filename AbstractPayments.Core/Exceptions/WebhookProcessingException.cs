namespace AbstractPayments.Core.Exceptions;

using System;

/// <summary>
/// Exception thrown when the webhook event processing handler fails and exceeds the configured retry threshold.
/// </summary>
public class WebhookProcessingException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookProcessingException"/> class.
    /// </summary>
    /// <param name="provider">The provider name.</param>
    /// <param name="innerException">The original exception thrown by the event handler.</param>
    public WebhookProcessingException(string provider, Exception innerException)
        : base($"Failed to process webhook event for provider '{provider}' after exhausting retry attempts.", innerException)
    {}
}
