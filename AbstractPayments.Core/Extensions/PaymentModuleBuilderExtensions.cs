namespace AbstractPayments.Core.Extensions;

using System;
using System.Reflection;
using AbstractPayments.Core.Abstractions;
using AbstractPayments.Core.Abstractions.Payments;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Fluent extension methods for IPaymentModuleBuilder to register composite capability gateway providers.
/// </summary>
public static class PaymentModuleBuilderExtensions
{
    /// <summary>
    /// Registers a specialized payment gateway capability provider under a dynamic composite service key (Prefix:name).
    /// </summary>
    /// <typeparam name="TContract">The specialized capability interface (e.g. IPixGateway).</typeparam>
    /// <typeparam name="TImpl">The concrete gateway capability implementation class.</typeparam>
    /// <param name="builder">The payments module builder.</param>
    /// <param name="name">The unique provider name identifier.</param>
    /// <returns>The module builder for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if TContract is not decorated with GatewayCapabilityAttribute.</exception>
    public static IPaymentModuleBuilder AddProvider<TContract, TImpl>(
        this IPaymentModuleBuilder builder,
        string name)
        where TContract : class, IPaymentGateway
        where TImpl : class, TContract
    {
        var attribute = typeof(TContract).GetCustomAttribute<GatewayCapabilityAttribute>();

        if (attribute == null)
        {
            throw new InvalidOperationException($"The contract type '{typeof(TContract).FullName}' must be decorated with '{nameof(GatewayCapabilityAttribute)}'.");
        }

        string serviceKey = $"{attribute.Prefix}:{name}";

        builder.Services.AddKeyedScoped<IPaymentGateway, TImpl>(serviceKey);
        builder.Services.AddKeyedScoped<TContract, TImpl>(serviceKey);

        return builder;
    }

    /// <summary>
    /// Semantically registers a Pix capability provider fluently under a unique Pix composite key (Pix:name).
    /// </summary>
    /// <typeparam name="TImpl">The concrete gateway Pix implementation class.</typeparam>
    /// <param name="builder">The payments module builder.</param>
    /// <param name="name">The unique provider name identifier.</param>
    /// <returns>The module builder for fluent chaining.</returns>
    public static IPaymentModuleBuilder AddPixProvider<TImpl>(
        this IPaymentModuleBuilder builder,
        string name)
        where TImpl : class, IPixGateway
    {
        return builder.AddProvider<IPixGateway, TImpl>(name);
    }
}
