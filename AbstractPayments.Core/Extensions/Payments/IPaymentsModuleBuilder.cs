namespace AbstractPayments.Core.Extensions.Payments;

using AbstractPayments.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Plural Payments Module builder allowing registering capability-specific plugins (Pix, CreditCard).
/// </summary>
public interface IPaymentsModuleBuilder
{
    /// <summary>
    /// Gets the application service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Gets the Pix capability plugin builder.
    /// </summary>
    IPaymentPluginBuilder<IPixGateway> Pix { get; }
}

/// <summary>
/// Concrete internal implementation of the plural payments module builder.
/// </summary>
internal class PaymentsModuleBuilder : IPaymentsModuleBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <inheritdoc />
    public IPaymentPluginBuilder<IPixGateway> Pix { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentsModuleBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public PaymentsModuleBuilder(IServiceCollection services)
    {
        Services = services;
        Pix = new PaymentPluginBuilder<IPixGateway>(services);
    }
}
