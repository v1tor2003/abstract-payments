namespace AbstractPayments.Core.Abstractions;

/// <summary>
/// Strategy factory for dynamically resolving registered payment gateway plugins by name and capability type.
/// </summary>
public interface IPaymentGatewayFactory
{
    /// <summary>
    /// Dynamically resolves the specific registered payment gateway implementation by name.
    /// </summary>
    /// <typeparam name="T">The specialized gateway capability interface (e.g. IPixPaymentGateway).</typeparam>
    /// <param name="name">The registered unique name of the gateway provider.</param>
    /// <returns>The resolved gateway plugin implementation.</returns>
    /// <exception cref="AbstractPayments.Core.Exceptions.GatewayNotRegisteredException">Thrown if no gateway is registered with that name.</exception>
    /// <exception cref="AbstractPayments.Core.Exceptions.GatewayTypeMismatchException">Thrown if the resolved gateway does not implement T.</exception>
    T Get<T>(string name) where T : class, IPaymentGateway;
}
