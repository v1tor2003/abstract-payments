namespace AbstractPayments.Core.Processors.Webhooks;

using System;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Core.Exceptions;
using AbstractPayments.Core.Extensions.Options;
using AbstractPayments.Core.Models.Webhooks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Core implementation of the IWebhookProcessor orchestration engine.
/// </summary>
public class WebhookProcessor : IWebhookProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebhookQueue _queue;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookProcessor"/> class.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    /// <param name="queue">The webhook queue.</param>
    public WebhookProcessor(IServiceProvider serviceProvider, IWebhookQueue queue)
    {
        _serviceProvider = serviceProvider;
        _queue = queue;
    }

    /// <inheritdoc />
    public async Task ProcessAsync(WebhookContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string provider = context.Provider;

        string validatorKey = $"validator:{provider}";
        var validator = _serviceProvider.GetKeyedService<IWebhookSignatureValidator>(validatorKey);
        
        if (validator == null)
        {
            throw new ProviderConfigurationException(provider, "IWebhookSignatureValidator");
        }

        bool isValid = await validator.ValidateAsync(context);
        if (!isValid)
        {
            throw new WebhookSignatureValidationException(provider);
        }

        string parserKey = $"parser:{provider}";
        var converter = _serviceProvider.GetKeyedService<IWebhookEventConverter>(parserKey);
        if (converter == null)
        {
            throw new ProviderConfigurationException(provider, "IWebhookEventConverter");
        }

        var @event = await converter.ConvertAsync(context);
        if (@event == null)
        {
            throw new InvalidOperationException($"Converter for provider '{provider}' returned a null WebhookEvent.");
        }

        // Delegate event processing to the queue on the consumer side
        await _queue.EnqueueAsync(@event);
    }
}
