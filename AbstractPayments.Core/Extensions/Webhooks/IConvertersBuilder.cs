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

/// <summary>
/// Concrete internal implementation of the converters builder.
/// </summary>
internal class ConvertersBuilder : IConvertersBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertersBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public ConvertersBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <inheritdoc />
    public IConvertersBuilder AddConverter<TConverter>(string provider)
        where TConverter : class, IWebhookEventConverter
    {
        string serviceKey = $"parser:{provider}";
        Services.AddKeyedScoped<IWebhookEventConverter, TConverter>(serviceKey);
        return this;
    }
}

