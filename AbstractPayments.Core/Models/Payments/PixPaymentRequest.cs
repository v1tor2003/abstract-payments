namespace AbstractPayments.Core.Models.Payments;

using System.Collections.Generic;

/// <summary>
/// Provider-agnostic domain model representing a Pix payment request.
/// </summary>
public record PixPaymentRequest(
    decimal Amount,
    string Description,
    string PayerDocument,
    Dictionary<string, object>? Metadata = null
);
