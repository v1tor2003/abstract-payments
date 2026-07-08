namespace AbstractPayments.Core.Abstractions.Webhooks;

using System.Threading.Tasks;
using AbstractPayments.Core.Models.Webhooks;

/// <summary>
/// Core engine orchestrator that coordinates the validation, conversion, and processing of provider webhook events.
/// </summary>
public interface IWebhookProcessor
{
    /// <summary>
    /// Authenticates, translates, and executes handling logic for the given webhook context.
    /// </summary>
    /// <param name="context">The raw webhook request context.</param>
    Task ProcessAsync(WebhookContext context);
}
