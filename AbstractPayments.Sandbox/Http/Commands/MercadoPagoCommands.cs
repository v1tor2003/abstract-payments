using System.Net.Http;
using System.Text.Json.Serialization;

namespace AbstractPayments.Sandbox.Http.Commands;

public record MercadoPagoPayerIdentification(
    [property: JsonPropertyName("type")] string Type, 
    [property: JsonPropertyName("number")] string Number
);

public record MercadoPagoPayer(
    [property: JsonPropertyName("email")] string Email, 
    [property: JsonPropertyName("identification")] MercadoPagoPayerIdentification Identification
);

public record MercadoPagoPixRequest(
    [property: JsonPropertyName("transaction_amount")] decimal TransactionAmount,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("payment_method_id")] string PaymentMethodId,
    [property: JsonPropertyName("payer")] MercadoPagoPayer Payer
);

public record MercadoPagoTransactionData(
    [property: JsonPropertyName("qr_code")] string QrCode, 
    [property: JsonPropertyName("qr_code_base64")] string QrCodeBase64
);

public record MercadoPagoPointOfInteraction(
    [property: JsonPropertyName("transaction_data")] MercadoPagoTransactionData TransactionData
);

public record MercadoPagoPixResponse(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("point_of_interaction")] MercadoPagoPointOfInteraction PointOfInteraction
);

public class CreateMercadoPagoPixCommand : ApiCommand<MercadoPagoPixRequest, MercadoPagoPixResponse>
{
    public CreateMercadoPagoPixCommand(MercadoPagoPixRequest request) : base(request)
    {
    }

    public override HttpMethod Method => HttpMethod.Post;
    public override string Endpoint => "/v1/payments";
}
