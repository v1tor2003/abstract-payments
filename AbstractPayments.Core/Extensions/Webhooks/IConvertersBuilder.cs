namespace AbstractPayments.Core.Extensions.Webhooks;

using AbstractPayments.Core.Abstractions.Webhooks;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Sub-builder contract for registering webhook event converter/parser strategies.
/// </summary>
public interface IConvertersBuilder
{
    /// <summary>
    /// Gets the application service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Registers an event converter for a specific provider.
    /// </summary>
    /// <typeparam name="TConverter">The concrete converter type.</typeparam>
    /// <param name="provider">The provider name.</param>
    /// <returns>This builder for method chaining.</returns>
    IConvertersBuilder AddConverter<TConverter>(string provider)
        where TConverter : class, IWebhookEventConverter;
}
