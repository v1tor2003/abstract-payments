namespace AbstractPayments.Core.Models;

/// <summary>
/// Represents a standardized, provider-agnostic payment execution error.
/// </summary>
/// <param name="Code">A unified error code (e.g., "INSUFFICIENT_FUNDS").</param>
/// <param name="Message">A developer or user-friendly error message.</param>
/// <param name="ProviderError">The raw string error returned from the provider SDK for debugging.</param>
public record PaymentError(string Code, string Message, string? ProviderError = null);
