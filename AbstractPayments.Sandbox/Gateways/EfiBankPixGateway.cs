using System;
using System.Globalization;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions;
using AbstractPayments.Core.Models.Payments;
using AbstractPayments.Sandbox.Http;
using AbstractPayments.Sandbox.Http.Commands;

namespace AbstractPayments.Sandbox.Gateways;

/// <summary>
/// Framework-compliant adapter implementing IPixGateway using EfiBank API commands.
/// </summary>
public class EfiBankPixGateway : IPixGateway
{
    private readonly ApiClient _apiClient;

    /// <inheritdoc />
    public string Name => "efibank";

    /// <summary>
    /// Initializes a new instance of the <see cref="EfiBankPixGateway"/> class.
    /// </summary>
    public EfiBankPixGateway(ApiClient apiClient)
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

        var amountString = pixRequest.Amount.ToString("F2", CultureInfo.InvariantCulture);
        var efiRequest = new EfiBankPixRequest(
            Calendario: new EfiBankCalendarioRequest(3600),
            Devedor: new EfiBankDevedor(pixRequest.PayerDocument, "Payer Abstracted EfiBank"),
            Valor: new EfiBankValor(amountString),
            Chave: "efibank-pix-key-sample-1234"
        );

        var command = new CreateEfiBankPixCommand(efiRequest);
        var response = await _apiClient.SendAsync(command);

        PixPaymentResult result;
        if (response == null)
        {
            result = new PixPaymentResult(
                Success: false,
                ExternalId: string.Empty,
                QrCode: string.Empty,
                QrCodeImage: string.Empty,
                Error: new AbstractPayments.Core.Models.PaymentError("GATEWAY_ERROR", "EfiBank did not return a valid payload.")
            );
        }
        else
        {
            result = new PixPaymentResult(
                Success: true,
                ExternalId: response.Txid,
                QrCode: response.PixCopiaECola,
                QrCodeImage: "efi-mock-qr-code-image-url"
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

        var result = "efibank-refund-xyz";
        return Task.FromResult((TResponse)(object)result);
    }
}
