using System;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions;
using AbstractPayments.Core.Models.Payments;
using AbstractPayments.Sandbox.Http;
using AbstractPayments.Sandbox.Http.Commands;

namespace AbstractPayments.Sandbox.Gateways;

/// <summary>
/// Framework-compliant adapter implementing IPixGateway using Mercado Pago API commands.
/// </summary>
public class MercadoPagoPixGateway : IPixGateway
{
    private readonly ApiClient _apiClient;

    /// <inheritdoc />
    public string Name => "mercadopago";

    /// <summary>
    /// Initializes a new instance of the <see cref="MercadoPagoPixGateway"/> class.
    /// </summary>
    public MercadoPagoPixGateway(ApiClient apiClient)
    {
        _apiClient = apiClient;
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

        var mpRequest = new MercadoPagoPixRequest(
            TransactionAmount: pixRequest.Amount,
            Description: pixRequest.Description,
            PaymentMethodId: "pix",
            Payer: new MercadoPagoPayer(
                Email: "payer@abstracted.com",
                Identification: new MercadoPagoPayerIdentification("CPF", pixRequest.PayerDocument)
            )
        );

        var command = new CreateMercadoPagoPixCommand(mpRequest);
        var response = await _apiClient.SendAsync(command);

        PixPaymentResult result;
        if (response == null)
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
                ExternalId: response.Id.ToString(),
                QrCode: response.PointOfInteraction.TransactionData.QrCode,
                QrCodeImage: response.PointOfInteraction.TransactionData.QrCodeBase64
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
