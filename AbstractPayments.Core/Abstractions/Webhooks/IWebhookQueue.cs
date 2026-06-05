namespace AbstractPayments.Core.Abstractions.Webhooks;

using System.Threading;
using System.Threading.Tasks;
using AbstractPayments.Core.Models.Webhooks;

/// <summary>
/// Defines the asynchronous queue contract to yield validated and converted webhook events to the consumer side.
/// </summary>
public interface IWebhookQueue
{
    /// <summary>
    /// Enqueues a webhook event for asynchronous handling.
    /// </summary>
    /// <param name="event">The webhook event to enqueue.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    ValueTask EnqueueAsync(WebhookEvent @event, CancellationToken cancellationToken = default);
}
