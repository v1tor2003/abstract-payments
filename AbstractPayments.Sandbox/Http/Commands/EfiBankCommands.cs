using System.Net.Http;
using System.Text.Json.Serialization;

namespace AbstractPayments.Sandbox.Http.Commands;

public record EfiBankCalendarioRequest(
    [property: JsonPropertyName("expiracao")] int Expiracao
);

public record EfiBankDevedor(
    [property: JsonPropertyName("cpf")] string Cpf, 
    [property: JsonPropertyName("nome")] string Nome
);

public record EfiBankValor(
    [property: JsonPropertyName("original")] string Original
);

public record EfiBankPixRequest(
    [property: JsonPropertyName("calendario")] EfiBankCalendarioRequest Calendario,
    [property: JsonPropertyName("devedor")] EfiBankDevedor Devedor,
    [property: JsonPropertyName("valor")] EfiBankValor Valor,
    [property: JsonPropertyName("chave")] string Chave
);

public record EfiBankPixResponse(
    [property: JsonPropertyName("txid")] string Txid,
    [property: JsonPropertyName("pixCopiaECola")] string PixCopiaECola,
    [property: JsonPropertyName("status")] string Status
);

public class CreateEfiBankPixCommand : ApiCommand<EfiBankPixRequest, EfiBankPixResponse>
{
    public CreateEfiBankPixCommand(EfiBankPixRequest request) : base(request)
    {
    }

    public override HttpMethod Method => HttpMethod.Post;
    public override string Endpoint => "/v2/cob";
}
