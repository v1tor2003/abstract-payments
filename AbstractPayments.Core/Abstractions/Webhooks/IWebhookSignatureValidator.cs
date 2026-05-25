namespace AbstractPayments.Core.Abstractions.Webhooks;

using System.Threading.Tasks;
using AbstractPayments.Core.Models.Webhooks;

/// <summary>
/// Strategy contract for validating the signature authenticity of incoming provider webhooks.
/// </summary>
public interface IWebhookSignatureValidator
{
    /// <summary>
    /// Validates the authenticity of the webhook request.
    /// </summary>
    /// <param name="context">The incoming webhook context.</param>
    /// <returns>True if the request has a valid signature, otherwise false.</returns>
    Task<bool> ValidateAsync(WebhookContext context);
}
