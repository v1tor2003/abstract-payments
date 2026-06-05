namespace AbstractPayments.Core.Models.Payments;

/// <summary>
/// Provider-agnostic domain model representing a Pix payment response.
/// </summary>
public record PixPaymentResult(
    bool Success,
    string ExternalId,
    string QrCode,
    string QrCodeImage,
    PaymentError? Error = null
);
