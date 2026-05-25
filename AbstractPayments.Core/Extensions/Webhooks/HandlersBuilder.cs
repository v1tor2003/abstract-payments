namespace AbstractPayments.Core.Extensions.Webhooks;

using AbstractPayments.Core.Abstractions.Webhooks;
using Microsoft.Extensions.DependencyInjection;

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
