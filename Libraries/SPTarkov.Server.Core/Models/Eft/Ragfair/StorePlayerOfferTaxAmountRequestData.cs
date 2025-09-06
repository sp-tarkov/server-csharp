using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Ragfair;

public record StorePlayerOfferTaxAmountRequestData : IRequestData
{
    [JsonPropertyName("id")]
    public MongoId? Id { get; set; }

    [JsonPropertyName("tpl")]
    public MongoId? Tpl { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("fee")]
    public double? Fee { get; set; }
}
