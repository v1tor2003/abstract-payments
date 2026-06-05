namespace AbstractPayments.Sandbox.Requests;

/// <summary>
/// Represents a coupled Pix payment request payload.
/// </summary>
public record CoupledPixRequest(decimal Amount, string? Provider = null);
