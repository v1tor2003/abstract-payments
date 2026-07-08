namespace AbstractPayments.Core.Extensions.Payments;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Fluent configuration builder wrapping the application service collection.
/// </summary>
public interface IAbstractPaymentsBuilder
{
    /// <summary>
    /// Gets the application service collection.
    /// </summary>
    IServiceCollection Services { get; }
}

/// <summary>
/// Concrete internal implementation of the abstract payments builder.
/// </summary>
internal class AbstractPaymentsBuilder : IAbstractPaymentsBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractPaymentsBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public AbstractPaymentsBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
