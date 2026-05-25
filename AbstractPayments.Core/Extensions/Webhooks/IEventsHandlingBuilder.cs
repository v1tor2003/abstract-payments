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

/// <summary>
/// Concrete internal implementation of the events handling builder.
/// </summary>
internal class EventsHandlingBuilder : IEventsHandlingBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <inheritdoc />
    public string Endpoint { get; set; } = "/v1/api/payments/webhook";

    /// <inheritdoc />
    public int RetryCount { get; set; } = 3;

    /// <inheritdoc />
    public TimeSpan TimeSkewWindow { get; set; } = TimeSpan.FromMinutes(5);

    /// <inheritdoc />
    public ISignatureValidatorsBuilder SignatureValidators { get; }

    /// <inheritdoc />
    public IConvertersBuilder Converters { get; }

    /// <inheritdoc />
    public IHandlersBuilder Handlers { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventsHandlingBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public EventsHandlingBuilder(IServiceCollection services)
    {
        Services = services;
        SignatureValidators = new SignatureValidatorsBuilder(services);
        Converters = new ConvertersBuilder(services);
        Handlers = new HandlersBuilder(services);
    }
}
