namespace AbstractPayments.Sandbox.Requests;

/// <summary>
/// Represents a framework-decoupled Pix payment request payload.
/// </summary>
public record AbstractedPixRequest(decimal Amount, string Provider);
