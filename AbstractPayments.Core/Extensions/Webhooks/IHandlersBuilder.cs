namespace AbstractPayments.Core.Extensions.Webhooks;

using AbstractPayments.Core.Abstractions.Webhooks;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Sub-builder contract for registering custom webhook event handlers.
/// </summary>
public interface IHandlersBuilder
{
    /// <summary>
    /// Gets the application service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Registers a custom event handler for a specific provider.
    /// </summary>
    /// <typeparam name="THandler">The concrete handler type.</typeparam>
    /// <param name="provider">The provider name.</param>
    /// <returns>This builder for method chaining.</returns>
    IHandlersBuilder AddHandler<THandler>(string provider)
        where THandler : class, IWebhookEventHandler;
}

/// <summary>
/// Concrete internal implementation of the handlers builder.
/// </summary>
internal class HandlersBuilder : IHandlersBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlersBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public HandlersBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <inheritdoc />
    public IHandlersBuilder AddHandler<THandler>(string provider)
        where THandler : class, IWebhookEventHandler
    {
        string serviceKey = $"handler:{provider}";
        Services.AddKeyedScoped<IWebhookEventHandler, THandler>(serviceKey);
        return this;
    }
}
