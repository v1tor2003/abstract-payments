namespace AbstractPayments.Core.Extensions.Payments;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Nested module-level fluent builder representing the Payments domain capability scope.
/// </summary>
public interface IPaymentModuleBuilder
{
    /// <summary>   
    /// Gets the application service collection.
    /// </summary>
    IServiceCollection Services { get; }
}

/// <summary>
/// Concrete internal implementation of the payment module builder.
/// </summary>
internal class PaymentModuleBuilder : IPaymentModuleBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentModuleBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public PaymentModuleBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
