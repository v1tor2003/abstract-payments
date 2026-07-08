namespace AbstractPayments.Sandbox.Gateways.Models.Bacen;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the BACEN cob (immediate charge) calendar details.
/// </summary>
public class PixCalendario
{
    /// <summary>
    /// Gets or sets the expiration time in seconds.
    /// </summary>
    [JsonPropertyName("expiracao")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Expiracao { get; set; }
}

/// <summary>
/// Represents the BACEN cob debtor details.
/// </summary>
public class PixDevedor
{
    /// <summary>
    /// Gets or sets the debtor's CPF.
    /// </summary>
    [JsonPropertyName("cpf")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Cpf { get; set; }

    /// <summary>
    /// Gets or sets the debtor's CNPJ.
    /// </summary>
    [JsonPropertyName("cnpj")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Cnpj { get; set; }

    /// <summary>
    /// Gets or sets the debtor's full legal name.
    /// </summary>
    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;
}

/// <summary>
/// Represents the BACEN cob transaction value details.
/// </summary>
public class PixValor
{
    /// <summary>
    /// Gets or sets the original value of the immediate charge.
    /// </summary>
    [JsonPropertyName("original")]
    [JsonConverter(typeof(DecimalStringConverter))]
    public decimal Original { get; set; }
}

/// <summary>
/// Represents the official BACEN cob (Immediate Charge) payload standard.
/// </summary>
public class PixChargeRequest
{
    /// <summary>
    /// Gets or sets the calendar details of the Pix charge.
    /// </summary>
    [JsonPropertyName("calendario")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PixCalendario? Calendario { get; set; }

    /// <summary>
    /// Gets or sets the debtor of the Pix charge.
    /// </summary>
    [JsonPropertyName("devedor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PixDevedor? Devedor { get; set; }

    /// <summary>
    /// Gets or sets the amount metadata.
    /// </summary>
    [JsonPropertyName("valor")]
    public PixValor Valor { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DICT key of the merchant.
    /// </summary>
    [JsonPropertyName("chave")]
    public string Chave { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional direct message visible to the buyer.
    /// </summary>
    [JsonPropertyName("solicitacaoPagador")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SolicitacaoPagador { get; set; }
}
