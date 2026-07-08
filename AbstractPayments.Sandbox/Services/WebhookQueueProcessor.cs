namespace AbstractPayments.Sandbox.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Core.Extensions.Options;
using AbstractPayments.Core.Models.Webhooks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Background worker that continually processes webhooks dequeued from the in-memory queue.
/// </summary>
public class WebhookQueueProcessor : BackgroundService
{
    private readonly InMemoryWebhookQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<WebhookOptions> _options;
    private readonly ILogger<WebhookQueueProcessor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookQueueProcessor"/> class.
    /// </summary>
    public WebhookQueueProcessor(
        InMemoryWebhookQueue queue,
        IServiceProvider serviceProvider,
        IOptions<WebhookOptions> options,
        ILogger<WebhookQueueProcessor> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Webhook Queue Processor background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var @event = await _queue.DequeueAsync(stoppingToken);
                _logger.LogInformation("Processing webhook event {EventId} for provider {Provider} from queue.", @event.EventId, @event.Provider);

                // Open a new dependency injection scope for executing the scoped event handler (and storage/repos)
                using var scope = _serviceProvider.CreateScope();
                
                string handlerKey = $"handler:{@event.Provider}";
                var handler = scope.ServiceProvider.GetKeyedService<IWebhookEventHandler>(handlerKey);
                
                if (handler == null)
                {
                    _logger.LogError("No webhook event handler registered for provider '{Provider}'. Event {EventId} discarded.", @event.Provider, @event.EventId);
                    continue;
                }

                int retryLimit = _options.Value?.RetryCount ?? 0;
                int attempt = 0;
                Exception? lastException = null;
                bool success = false;

                while (attempt <= retryLimit && !stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await handler.HandleAsync(@event);
                        success = true;
                        _logger.LogInformation("Successfully processed webhook event {EventId} on attempt {Attempt}.", @event.EventId, attempt + 1);
                        break;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        attempt++;
                        _logger.LogWarning(ex, "Attempt {Attempt} failed to handle webhook event {EventId}.", attempt, @event.EventId);
                    }
                }

                if (!success)
                {
                    _logger.LogError(lastException, "Failed to process webhook event {EventId} after {Attempts} attempts.", @event.EventId, attempt);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred in the webhook queue processor loop.");
            }
        }

        _logger.LogInformation("Webhook Queue Processor background service stopped.");
    }
}
