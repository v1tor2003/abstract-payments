namespace AbstractPayments.Core.Abstractions.Webhooks;

using System.Threading.Tasks;
using AbstractPayments.Core.Models.Webhooks;

/// <summary>
/// Strategy contract for parsing and converting a raw provider webhook payload into a normalized domain WebhookEvent.
/// </summary>
public interface IWebhookEventConverter
{
    /// <summary>
    /// Converts the raw provider payload into the unified WebhookEvent model.
    /// </summary>
    /// <param name="context">The raw incoming webhook context.</param>
    /// <returns>The normalized domain event representation.</returns>
    Task<WebhookEvent> ConvertAsync(WebhookContext context);
}
