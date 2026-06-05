namespace AbstractPayments.Core.Abstractions;

using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions.Payments;
using AbstractPayments.Core.Models.Payments;

/// <summary>
/// Specialized gateway contract representing a Pix capability gateway.
/// </summary>
[GatewayCapability("Pix")]
public interface IPixGateway : IPaymentGateway
{
    /// <summary>
    /// Generates a Pix payment for the specified request.
    /// </summary>
    Task<TResponse> GeneratePaymentAsync<TRequest, TResponse>(TRequest request)
        where TRequest : class
        where TResponse : class;

    /// <summary>
    /// Gets a refund status or execution for Pix.
    /// </summary>
    Task<TResponse> GetRefundAsync<TRequest, TResponse>(TRequest request)
        where TRequest : class
        where TResponse : class;
}

