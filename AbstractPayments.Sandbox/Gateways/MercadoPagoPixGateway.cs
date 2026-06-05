using System;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions;
using AbstractPayments.Core.Models.Payments;
using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;

namespace AbstractPayments.Sandbox.Gateways;

/// <summary>
/// Framework-compliant adapter implementing IPixGateway wrapping the official Mercado Pago SDK.
/// </summary>
public class MercadoPagoPixGateway : IPixGateway
{
    /// <inheritdoc />
    public string Name => "mercadopago";

    /// <summary>
    /// Initializes a new instance of the <see cref="MercadoPagoPixGateway"/> class.
    /// </summary>
    public MercadoPagoPixGateway()
    {
    }

    /// <inheritdoc />
    public async Task<TResponse> GeneratePaymentAsync<TRequest, TResponse>(TRequest request)
        where TRequest : class
        where TResponse : class
    {
        if (request is not PixPaymentRequest pixRequest)
        {
            throw new ArgumentException($"Unsupported request type: {typeof(TRequest).Name}", nameof(request));
        }

        if (typeof(TResponse) != typeof(PixPaymentResult))
        {
            throw new ArgumentException($"Unsupported response type: {typeof(TResponse).Name}");
        }

        if (string.IsNullOrEmpty(MercadoPagoConfig.AccessToken))
        {
            MercadoPagoConfig.AccessToken = "TEST-DecoupledAccessToken";
        }

        var mpRequest = new PaymentCreateRequest
        {
            TransactionAmount = pixRequest.Amount,
            Description = pixRequest.Description,
            PaymentMethodId = "pix",
            Payer = new PaymentPayerRequest
            {
                Email = "payer@abstracted.com",
                Identification = new IdentificationRequest
                {
                    Type = "CPF",
                    Number = pixRequest.PayerDocument
                }
            }
        };

        var client = new PaymentClient();
        Payment payment = await client.CreateAsync(mpRequest);

        PixPaymentResult result;
        if (payment == null)
        {
            result = new PixPaymentResult(
                Success: false,
                ExternalId: string.Empty,
                QrCode: string.Empty,
                QrCodeImage: string.Empty,
                Error: new AbstractPayments.Core.Models.PaymentError("GATEWAY_ERROR", "Mercado Pago did not return a payload.")
            );
        }
        else
        {
            result = new PixPaymentResult(
                Success: true,
                ExternalId: payment.Id?.ToString() ?? string.Empty,
                QrCode: payment.PointOfInteraction?.TransactionData?.QrCode ?? string.Empty,
                QrCodeImage: payment.PointOfInteraction?.TransactionData?.QrCodeBase64 ?? string.Empty
            );
        }

        return (TResponse)(object)result;
    }

    /// <inheritdoc />
    public Task<TResponse> GetRefundAsync<TRequest, TResponse>(TRequest request)
        where TRequest : class
        where TResponse : class
    {
        if (typeof(TResponse) != typeof(string))
        {
            throw new ArgumentException($"Unsupported response type: {typeof(TResponse).Name}");
        }

        var result = "mercadopago-refund-xyz";
        return Task.FromResult((TResponse)(object)result);
    }
}
