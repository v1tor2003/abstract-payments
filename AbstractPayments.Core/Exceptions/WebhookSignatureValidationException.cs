namespace AbstractPayments.Core.Exceptions;

using System;

/// <summary>
/// Exception thrown when an incoming webhook request fails cryptographic signature validation.
/// </summary>
public class WebhookSignatureValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookSignatureValidationException"/> class.
    /// </summary>
    /// <param name="provider">The provider name whose signature failed validation.</param>
    public WebhookSignatureValidationException(string provider)
        : base($"Webhook signature validation failed for provider '{provider}'.")
    {}
}
