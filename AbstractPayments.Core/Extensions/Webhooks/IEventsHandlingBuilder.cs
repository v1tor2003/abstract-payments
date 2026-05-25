namespace AbstractPayments.Core.Extensions.Webhooks;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Fluent builder representing the webhook event handling scope.
/// </summary>
public interface IEventsHandlingBuilder
{
    /// <summary>
    /// Gets the application service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Gets or sets the webhook ingestion HTTP route endpoint.
    /// </summary>
    string Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the number of retry attempts for failed handlers.
    /// </summary>
    int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the allowed time skew window to prevent replay or spamming attacks.
    /// </summary>
    TimeSpan TimeSkewWindow { get; set; }

    /// <summary>
    /// Gets the signature validators builder.
    /// </summary>
    ISignatureValidatorsBuilder SignatureValidators { get; }

    /// <summary>
    /// Gets the event converters builder.
    /// </summary>
    IConvertersBuilder Converters { get; }

    /// <summary>
    /// Gets the event handlers builder.
    /// </summary>
    IHandlersBuilder Handlers { get; }
}
