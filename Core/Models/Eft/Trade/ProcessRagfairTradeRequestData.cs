using System.Text.Json.Serialization;

namespace Core.Models.Eft.Trade;

public record ProcessRagfairTradeRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; }

    [JsonPropertyName("offers")]
    public List<OfferRequest>? Offers { get; set; }
}

public record OfferRequest
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("items")]
    public List<ItemRequest>? Items { get; set; }
}

public record ItemRequest
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}
