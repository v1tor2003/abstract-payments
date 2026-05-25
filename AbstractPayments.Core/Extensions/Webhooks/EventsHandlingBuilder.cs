namespace AbstractPayments.Core.Extensions.Webhooks;

using Microsoft.Extensions.DependencyInjection;

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
