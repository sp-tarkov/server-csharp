using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Ragfair;

public record StorePlayerOfferTaxAmountRequestData : IRequestData
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
