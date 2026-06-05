using System;
using System.Threading.Tasks;
using AbstractPayments.Sandbox.Http.Commands;
using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;

namespace AbstractPayments.Sandbox.Coupled;

/// <summary>
/// Direct coupled integration client for Mercado Pago.
/// </summary>
public class MercadoPagoDirectClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MercadoPagoDirectClient"/> class.
    /// </summary>
    public MercadoPagoDirectClient()
    {
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

        if (string.IsNullOrEmpty(MercadoPagoConfig.AccessToken))
        {
            MercadoPagoConfig.AccessToken = "TEST-CoupledAccessToken";
        }

        var request = new PaymentCreateRequest
        {
            TransactionAmount = amount,
            Description = "Direct coupled payment Mercado Pago",
            PaymentMethodId = "pix",
            Payer = new PaymentPayerRequest
            {
                Email = "payer@coupled.com",
                Identification = new IdentificationRequest
                {
                    Type = "CPF",
                    Number = "12345678909"
                }
            }
        };

        var client = new PaymentClient();
        Payment payment = await client.CreateAsync(request);

        if (payment == null)
        {
            throw new InvalidOperationException("Failed to generate payment from Mercado Pago direct client");
        }

        return new MercadoPagoPixResponse(
            Id: payment.Id ?? 0,
            Status: payment.Status,
            PointOfInteraction: new MercadoPagoPointOfInteraction(
                TransactionData: new MercadoPagoTransactionData(
                    QrCode: payment.PointOfInteraction?.TransactionData?.QrCode ?? string.Empty,
                    QrCodeBase64: payment.PointOfInteraction?.TransactionData?.QrCodeBase64 ?? string.Empty
                )
            )
        );
    }
}
