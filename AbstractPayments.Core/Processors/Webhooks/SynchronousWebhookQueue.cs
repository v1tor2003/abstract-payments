namespace AbstractPayments.Core.Processors.Webhooks;

using System;
using System.Threading;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Core.Exceptions;
using AbstractPayments.Core.Extensions.Options;
using AbstractPayments.Core.Models.Webhooks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Default synchronous implementation of <see cref="IWebhookQueue"/> to preserve backward-compatible inline processing.
/// </summary>
public class SynchronousWebhookQueue : IWebhookQueue
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<WebhookOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronousWebhookQueue"/> class.
    /// </summary>
    public SynchronousWebhookQueue(IServiceProvider serviceProvider, IOptions<WebhookOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    /// <inheritdoc />
    public async ValueTask EnqueueAsync(WebhookEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        using var scope = _serviceProvider.CreateScope();
        string handlerKey = $"handler:{@event.Provider}";
        var handler = scope.ServiceProvider.GetKeyedService<IWebhookEventHandler>(handlerKey);
        
        if (handler == null)
        {
            throw new ProviderConfigurationException(@event.Provider, "IWebhookEventHandler");
        }

        int retryLimit = _options.Value?.RetryCount ?? 0;
        int attempt = 0;
        Exception? lastException = null;

        while (attempt <= retryLimit && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                await handler.HandleAsync(@event);
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempt++;
            }
        }

        throw new WebhookProcessingException(@event.Provider, lastException!);
    }
}
