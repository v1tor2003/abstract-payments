using System;
using System.Globalization;
using System.Threading.Tasks;
using AbstractPayments.Sandbox.Http;
using AbstractPayments.Sandbox.Http.Commands;

namespace AbstractPayments.Sandbox.Coupled;

/// <summary>
/// Direct coupled integration client for EfiBank.
/// </summary>
public class EfiBankDirectClient
{
    private readonly ApiClient _apiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfiBankDirectClient"/> class.
    /// </summary>
    public EfiBankDirectClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Invokes the EfiBank API directly without using framework abstractions.
    /// </summary>
    public async Task<EfiBankPixResponse> CreatePixPaymentAsync(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        }

        var amountString = amount.ToString("F2", CultureInfo.InvariantCulture);
        var request = new EfiBankPixRequest(
            Calendario: new EfiBankCalendarioRequest(3600),
            Devedor: new EfiBankDevedor("12345678909", "Payer Coupled EfiBank"),
            Valor: new EfiBankValor(amountString),
            Chave: "efibank-pix-key-sample-1234"
        );

        var command = new CreateEfiBankPixCommand(request);
        var response = await _apiClient.SendAsync(command);

        if (response == null)
        {
            throw new InvalidOperationException("Failed to generate payment from EfiBank direct client");
        }

        return response;
    }
}
