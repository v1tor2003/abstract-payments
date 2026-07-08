using System;
using System.Threading.Tasks;
using AbstractPayments.Sandbox.Http;
using AbstractPayments.Sandbox.Http.Commands;

namespace AbstractPayments.Sandbox.Coupled;

/// <summary>
/// Direct coupled integration client for PagSeguro.
/// </summary>
public class PagSeguroDirectClient
{
    private readonly ApiClient _apiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="PagSeguroDirectClient"/> class.
    /// </summary>
    public PagSeguroDirectClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Invokes the PagSeguro API directly without using framework abstractions.
    /// </summary>
    public async Task<PagSeguroPixResponse> CreatePixPaymentAsync(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        }

        var cents = (int)(amount * 100);
        var request = new PagSeguroPixRequest(
            ReferenceId: "pagseguro_" + Guid.NewGuid().ToString("N")[..8],
            Customer: new PagSeguroCustomer("Payer Coupled", "payer@coupled.com", "12345678909"),
            QrCodes: new[]
            {
                new PagSeguroQrCodeRequest(
                    Amount: new PagSeguroAmount(cents),
                    ExpirationDate: DateTime.UtcNow.AddHours(1).ToString("o")
                )
            }
        );

        var command = new CreatePagSeguroPixCommand(request);
        var response = await _apiClient.SendAsync(command);

        if (response == null)
        {
            throw new InvalidOperationException("Failed to generate payment from PagSeguro direct client");
        }

        return response;
    }
}
