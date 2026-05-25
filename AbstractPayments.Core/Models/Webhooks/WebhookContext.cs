namespace AbstractPayments.Core.Models.Webhooks;

using System.Collections.Generic;

/// <summary>
/// Represents the raw contextual payload of an incoming payment gateway webhook request.
/// </summary>
public class WebhookContext
{
    /// <summary>
    /// Gets the target provider name (e.g. "mercadopago").
    /// </summary>
    public string Provider { get; }

    /// <summary>
    /// Gets the raw request body.
    /// </summary>
    public string Body { get; }

    /// <summary>
    /// Gets the incoming HTTP headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookContext"/> class.
    /// </summary>
    /// <param name="provider">The gateway provider name.</param>
    /// <param name="body">The raw body payload.</param>
    /// <param name="headers">The raw HTTP request headers.</param>
    public WebhookContext(string provider, string body, IReadOnlyDictionary<string, string> headers)
    {
        Provider = provider;
        Body = body;
        Headers = headers;
    }
}
