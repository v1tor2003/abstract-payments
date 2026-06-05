namespace AbstractPayments.Core.Extensions;

using System;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Core.Extensions.Webhooks;

/// <summary>
/// Options for configuring a specific webhook listener/provider.
/// </summary>
public interface IWebhookListenerOptions
{
    /// <summary>
    /// Configures the signature validator strategy for the webhook listener.
    /// </summary>
    IWebhookListenerOptions UseSignatureValidator<TValidator>()
        where TValidator : class, IWebhookSignatureValidator;

    /// <summary>
    /// Configures the event converter strategy for the webhook listener.
    /// </summary>
    IWebhookListenerOptions UseConverter<TConverter>()
        where TConverter : class, IWebhookEventConverter;

    /// <summary>
    /// Configures the event handler strategy for the webhook listener.
    /// </summary>
    IWebhookListenerOptions UseHandler<THandler>()
        where THandler : class, IWebhookEventHandler;
}

/// <summary>
/// Internal implementation of <see cref="IWebhookListenerOptions"/>.
/// </summary>
internal class WebhookListenerOptions : IWebhookListenerOptions
{
    private readonly IEventsHandlingBuilder _builder;
    private readonly string _provider;

    public WebhookListenerOptions(IEventsHandlingBuilder builder, string provider)
    {
        _builder = builder;
        _provider = provider;
    }

    /// <inheritdoc />
    public IWebhookListenerOptions UseSignatureValidator<TValidator>()
        where TValidator : class, IWebhookSignatureValidator
    {
        _builder.SignatureValidators.UseStrategy<TValidator>(_provider);
        return this;
    }

    /// <inheritdoc />
    public IWebhookListenerOptions UseConverter<TConverter>()
        where TConverter : class, IWebhookEventConverter
    {
        _builder.Converters.AddConverter<TConverter>(_provider);
        return this;
    }

    /// <inheritdoc />
    public IWebhookListenerOptions UseHandler<THandler>()
        where THandler : class, IWebhookEventHandler
    {
        _builder.Handlers.AddHandler<THandler>(_provider);
        return this;
    }
}

/// <summary>
/// Extension methods for <see cref="IEventsHandlingBuilder"/> to support fluent webhook listener configuration.
/// </summary>
public static class EventsHandlingBuilderExtensions
{
    /// <summary>
    /// Configures signature validation, event conversion, and processing handlers for a specific provider.
    /// </summary>
    /// <param name="builder">The events builder.</param>
    /// <param name="provider">The provider unique identifier/key.</param>
    /// <param name="configure">The options configuration callback.</param>
    /// <returns>The original events builder for fluent chaining.</returns>
    public static IEventsHandlingBuilder ListenFrom(
        this IEventsHandlingBuilder builder,
        string provider,
        Action<IWebhookListenerOptions> configure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new WebhookListenerOptions(builder, provider.ToLowerInvariant());
        configure(options);
        return builder;
    }
}
