using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ragfair;

public class StorePlayerOfferTaxAmountRequestData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("tpl")]
    public string? Tpl { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("fee")]
    public double? Fee { get; set; }
}