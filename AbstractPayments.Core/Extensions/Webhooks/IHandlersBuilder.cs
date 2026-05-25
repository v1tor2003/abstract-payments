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
