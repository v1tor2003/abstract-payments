using System;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions;
using AbstractPayments.Core.Models.Payments;
using AbstractPayments.Sandbox.Http;
using AbstractPayments.Sandbox.Http.Commands;

namespace AbstractPayments.Sandbox.Gateways;

/// <summary>
/// Framework-compliant adapter implementing IPixGateway using PagSeguro API commands.
/// </summary>
public class PagSeguroPixGateway : IPixGateway
{
    private readonly ApiClient _apiClient;

    /// <inheritdoc />
    public string Name => "pagseguro";

    /// <summary>
    /// Initializes a new instance of the <see cref="PagSeguroPixGateway"/> class.
    /// </summary>
    public PagSeguroPixGateway(ApiClient apiClient)
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

        var cents = (int)(pixRequest.Amount * 100);
        var psRequest = new PagSeguroPixRequest(
            ReferenceId: "ps_ref_" + Guid.NewGuid().ToString("N")[..8],
            Customer: new PagSeguroCustomer("Payer Abstracted", "payer@abstracted.com", pixRequest.PayerDocument),
            QrCodes: new[]
            {
                new PagSeguroQrCodeRequest(
                    Amount: new PagSeguroAmount(cents),
                    ExpirationDate: DateTime.UtcNow.AddHours(2).ToString("o")
                )
            }
        );

        var command = new CreatePagSeguroPixCommand(psRequest);
        var response = await _apiClient.SendAsync(command);

        PixPaymentResult result;
        if (response == null || response.QrCodes == null || response.QrCodes.Length == 0)
        {
            result = new PixPaymentResult(
                Success: false,
                ExternalId: string.Empty,
                QrCode: string.Empty,
                QrCodeImage: string.Empty,
                Error: new AbstractPayments.Core.Models.PaymentError("GATEWAY_ERROR", "PagSeguro did not return a valid payload.")
            );
        }
        else
        {
            result = new PixPaymentResult(
                Success: true,
                ExternalId: response.ReferenceId,
                QrCode: response.QrCodes[0].Text,
                QrCodeImage: response.QrCodes[0].Link
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

        var result = "pagseguro-refund-xyz";
        return Task.FromResult((TResponse)(object)result);
    }
}
