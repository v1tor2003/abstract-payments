using System;

namespace AbstractPayments.Sandbox.Responses;

/// <summary>
/// Represents a framework-decoupled Pix payment response payload.
/// </summary>
public record AbstractedPixResponse(
    string TransactionId,
    decimal Amount,
    string Provider,
    string PaymentString,
    string Status,
    DateTime CreatedAt
);
