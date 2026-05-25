namespace AbstractPayments.Core.Abstractions;

using System.Threading.Tasks;

/// <summary>
/// Specialized gateway contract representing a Pix capability gateway.
/// </summary>
[GatewayCapability("Pix")]
public interface IPixGateway : IPaymentGateway
{
    /// <summary>
    /// Generates a Pix payment.
    /// </summary>
    /// <returns>A QR code or payment string.</returns>
    Task<string> GeneratePaymentAsync();

    /// <summary>
    /// Gets a refund status or execution for Pix.
    /// </summary>
    /// <returns>A refund representation string.</returns>
    Task<string> GetRefundAsync();
}
