namespace AbstractPayments.Core.Abstractions.Payments;

using System;

/// <summary>
/// Defines a payment method capability supported by a gateway.
/// This attribute is used to register and query capabilities through the PaymentMethodResolver.
/// </summary>
/// <remarks>
/// This attribute marks an interface as a payment method capability. 
/// Gateways implementing this interface will be registered with this capability.
/// It is used by the `PaymentMethodResolver` for dynamic dispatch.
/// </remarks>
/// <example>
/// <code>
/// [GatewayCapability("Pix")]
/// public interface IPixPaymentMethod : IPaymentMethod { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Interface)]
public class GatewayCapabilityAttribute : Attribute
{
    /// <summary>
    /// Gets the unique string prefix (e.g. "Pix", "Card").
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayCapabilityAttribute"/> class.
    /// </summary>
    /// <param name="prefix">The dynamic resolution capability prefix.</param>
    public GatewayCapabilityAttribute(string prefix)
    {
        Prefix = prefix;
    }
}
