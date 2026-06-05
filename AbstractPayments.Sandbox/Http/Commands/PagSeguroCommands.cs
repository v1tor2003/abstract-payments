using System;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace AbstractPayments.Sandbox.Http.Commands;

public record PagSeguroCustomer(
    [property: JsonPropertyName("name")] string Name, 
    [property: JsonPropertyName("email")] string Email, 
    [property: JsonPropertyName("tax_id")] string TaxId
);

public record PagSeguroAmount(
    [property: JsonPropertyName("value")] int Value
);

public record PagSeguroQrCodeRequest(
    [property: JsonPropertyName("amount")] PagSeguroAmount Amount, 
    [property: JsonPropertyName("expiration_date")] string ExpirationDate
);

public record PagSeguroPixRequest(
    [property: JsonPropertyName("reference_id")] string ReferenceId,
    [property: JsonPropertyName("customer")] PagSeguroCustomer Customer,
    [property: JsonPropertyName("qr_codes")] PagSeguroQrCodeRequest[] QrCodes
);

public record PagSeguroQrCodeResponse(
    [property: JsonPropertyName("id")] string Id, 
    [property: JsonPropertyName("text")] string Text, 
    [property: JsonPropertyName("link")] string Link
);

public record PagSeguroPixResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("reference_id")] string ReferenceId,
    [property: JsonPropertyName("qr_codes")] PagSeguroQrCodeResponse[] QrCodes
);

public class CreatePagSeguroPixCommand : ApiCommand<PagSeguroPixRequest, PagSeguroPixResponse>
{
    public CreatePagSeguroPixCommand(PagSeguroPixRequest request) : base(request)
    {
    }

    public override HttpMethod Method => HttpMethod.Post;
    public override string Endpoint => "/orders";
}
