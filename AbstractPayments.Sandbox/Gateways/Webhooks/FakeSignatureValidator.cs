namespace AbstractPayments.Sandbox.Gateways.Webhooks;

using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Core.Models.Webhooks;

/// <summary>
/// Framework-compliant signature validator verifying incoming Fake Gateway webhook requests.
/// </summary>
public class FakeSignatureValidator : IWebhookSignatureValidator
{
    /// <inheritdoc />
    public Task<bool> ValidateAsync(WebhookContext context)
    {
        if (context.Headers.TryGetValue("X-Signature", out var signature))
        {
            return Task.FromResult(signature == "fake_secret");
        }
        return Task.FromResult(false);
    }
}
