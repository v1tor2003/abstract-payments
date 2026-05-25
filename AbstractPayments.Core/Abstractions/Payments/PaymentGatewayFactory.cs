namespace AbstractPayments.Core.Abstractions;

using System;
using System.Reflection;
using AbstractPayments.Core.Exceptions;
using AbstractPayments.Core.Abstractions.Payments;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Native Keyed-Services backed implementation of the payment gateway strategy resolver factory, enforcing strict capability prefix keys.
/// </summary>
public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentGatewayFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    public PaymentGatewayFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public T Get<T>(string name) where T : class, IPaymentGateway
    {
        var attribute = typeof(T).GetCustomAttribute<GatewayCapabilityAttribute>();

        if (attribute == null)
        {
            throw new InvalidOperationException($"The interface type '{typeof(T).FullName}' is not decorated with the required '{nameof(GatewayCapabilityAttribute)}'.");
        }

        string serviceKey = $"{attribute.Prefix}:{name}";
        var gateway = _serviceProvider.GetKeyedService<IPaymentGateway>(serviceKey);

        if (gateway == null)
        {
            throw new GatewayNotRegisteredException(serviceKey);
        }

        if (gateway is not T typedGateway)
        {
            throw new GatewayTypeMismatchException(serviceKey, typeof(T), gateway.GetType());
        }

        return typedGateway;
    }
}
