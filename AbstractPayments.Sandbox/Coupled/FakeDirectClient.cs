namespace AbstractPayments.Sandbox.Coupled;

using System;
using System.Threading.Tasks;

/// <summary>
/// Bespoke vendor response from the simulated Fake Gateway SDK.
/// </summary>
public class FakePixResponse
{
    /// <summary>
    /// Gets or sets the custom Fake Gateway transaction ID.
    /// </summary>
    public string FakeTransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vendor-specific amount property.
    /// </summary>
    public decimal MerchantAmount { get; set; }

    /// <summary>
    /// Gets or sets the raw Base64 QR code representation.
    /// </summary>
    public string QrCodeBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status string directly from the SDK.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets some direct metadata tracing response headers.
    /// </summary>
    public string ResponseMetadata { get; set; } = string.Empty;
}

/// <summary>
/// Concrete direct payment integration client bypassing any abstractions.
/// </summary>
public class FakeDirectClient
{
    /// <summary>
    /// Calls the simulated external SDK API directly.
    /// </summary>
    public Task<FakePixResponse> CreatePixPaymentAsync(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        }

        var fakeTxId = "fake_tx_" + Guid.NewGuid().ToString("N")[..12];
        var response = new FakePixResponse
        {
            FakeTransactionId = fakeTxId,
            MerchantAmount = amount,
            QrCodeBase64 = "fake-direct-qrcode-raw-base64-payload-xyz",
            Status = "created",
            ResponseMetadata = "fake-sdk-direct-call-success"
        };

        return Task.FromResult(response);
    }
}
