namespace AbstractPayments.Core.Abstractions;

/// <summary>
/// Core contract representing a stateless payment gateway provider.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Unique name of the gateway provider (e.g. "mercadopago", "stripe").
    /// </summary>
    string Name { get; }
}
