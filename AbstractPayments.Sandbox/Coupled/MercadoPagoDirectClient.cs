using System;
using System.Threading.Tasks;
using AbstractPayments.Sandbox.Http;
using AbstractPayments.Sandbox.Http.Commands;

namespace AbstractPayments.Sandbox.Coupled;

/// <summary>
/// Direct coupled integration client for Mercado Pago.
/// </summary>
public class MercadoPagoDirectClient
{
    private readonly ApiClient _apiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="MercadoPagoDirectClient"/> class.
    /// </summary>
    public MercadoPagoDirectClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Invokes the Mercado Pago API directly without using framework abstractions.
    /// </summary>
    public async Task<MercadoPagoPixResponse> CreatePixPaymentAsync(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        }

        var request = new MercadoPagoPixRequest(
            TransactionAmount: amount,
            Description: "Direct coupled payment Mercado Pago",
            PaymentMethodId: "pix",
            Payer: new MercadoPagoPayer(
                Email: "payer@coupled.com",
                Identification: new MercadoPagoPayerIdentification("CPF", "12345678909")
            )
        );

        var command = new CreateMercadoPagoPixCommand(request);
        var response = await _apiClient.SendAsync(command);

        if (response == null)
        {
            throw new InvalidOperationException("Failed to generate payment from Mercado Pago direct client");
        }

        return response;
    }
}
