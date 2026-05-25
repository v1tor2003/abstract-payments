namespace AbstractPayments.Core.Processors.Webhooks;

using System;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Core.Exceptions;
using AbstractPayments.Core.Extensions;
using AbstractPayments.Core.Models.Webhooks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Core implementation of the IWebhookProcessor orchestration engine.
/// </summary>
public class WebhookProcessor : IWebhookProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<WebhookOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookProcessor"/> class.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    /// <param name="options">The configured webhook options.</param>
    public WebhookProcessor(IServiceProvider serviceProvider, IOptions<WebhookOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    /// <inheritdoc />
    public async Task ProcessAsync(WebhookContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

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

        string handlerKey = $"handler:{provider}";
        var handler = _serviceProvider.GetKeyedService<IWebhookEventHandler>(handlerKey);
        if (handler == null)
        {
            throw new ProviderConfigurationException(provider, "IWebhookEventHandler");
        }

        int retryLimit = _options.Value?.RetryCount ?? 0;
        int attempt = 0;
        Exception? lastException = null;

        while (attempt <= retryLimit)
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

        throw new WebhookProcessingException(provider, lastException!);
    }
}
