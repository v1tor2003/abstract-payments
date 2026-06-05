namespace AbstractPayments.Sandbox.Services;

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Core.Models.Webhooks;

/// <summary>
/// In-memory queue implementation for yielding webhook events using System.Threading.Channels.
/// </summary>
public class InMemoryWebhookQueue : IWebhookQueue
{
    private readonly Channel<WebhookEvent> _channel;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryWebhookQueue"/> class.
    /// </summary>
    public InMemoryWebhookQueue()
    {
        // Unbounded channel for high-throughput in-memory queueing
        _channel = Channel.CreateUnbounded<WebhookEvent>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    /// <inheritdoc />
    public async ValueTask EnqueueAsync(WebhookEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        await _channel.Writer.WriteAsync(@event, cancellationToken);
    }

    /// <summary>
    /// Dequeues an event from the in-memory channel.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The dequeued WebhookEvent.</returns>
    public async ValueTask<WebhookEvent> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }
}
