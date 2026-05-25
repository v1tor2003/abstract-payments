namespace AbstractPayments.Core.Abstractions.Webhooks;

using System.Threading.Tasks;
using AbstractPayments.Core.Models.Webhooks;

/// <summary>
/// User-defined event handler executing custom business/persistence logic for normalized gateway webhook events.
/// </summary>
public interface IWebhookEventHandler
{
    /// <summary>
    /// Handles the normalized webhook event.
    /// </summary>
    /// <param name="event">The normalized standard webhook event.</param>
    Task HandleAsync(WebhookEvent @event);
}
