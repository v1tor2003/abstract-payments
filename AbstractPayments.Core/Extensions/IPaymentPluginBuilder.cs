namespace AbstractPayments.Core.Extensions;

using System;
using System.Reflection;
using AbstractPayments.Core.Abstractions;
using AbstractPayments.Core.Abstractions.Payments;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Sub-builder contract for registering provider implementations for a specific payment method capability.
/// </summary>
/// <typeparam name="TContract">The specialized gateway capability interface.</typeparam>
public interface IPaymentPluginBuilder<TContract> where TContract : class, IPaymentGateway
{
    /// <summary>
    /// Gets the application service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Registers a gateway provider implementation for this capability under its composite service key.
    /// </summary>
    /// <typeparam name="TImpl">The concrete implementation type.</typeparam>
    /// <param name="name">The registered gateway provider name.</param>
    /// <returns>This builder for method chaining.</returns>
    IPaymentPluginBuilder<TContract> AddProvider<TImpl>(string name) where TImpl : class, TContract;
}

/// <summary>
/// Concrete internal implementation of the payment plugin builder.
/// </summary>
/// <typeparam name="TContract">The specialized gateway capability interface.</typeparam>
internal class PaymentPluginBuilder<TContract> : IPaymentPluginBuilder<TContract> where TContract : class, IPaymentGateway
{
    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentPluginBuilder{TContract}"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public PaymentPluginBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <inheritdoc />
    public IPaymentPluginBuilder<TContract> AddProvider<TImpl>(string name) where TImpl : class, TContract
    {
        var attribute = typeof(TContract).GetCustomAttribute<GatewayCapabilityAttribute>();

        if (attribute == null)
        {
            throw new InvalidOperationException($"The contract type '{typeof(TContract).FullName}' must be decorated with '{nameof(GatewayCapabilityAttribute)}'.");
        }

        string serviceKey = $"{attribute.Prefix}:{name}";

        Services.AddKeyedScoped<IPaymentGateway, TImpl>(serviceKey);
        Services.AddKeyedScoped<TContract, TImpl>(serviceKey);

        return this;
    }
}
