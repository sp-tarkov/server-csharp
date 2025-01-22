using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Trade;

public record ProcessRagfairTradeRequestData : InventoryBaseActionRequestData
{
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
    public double? Count { get; set; }
}
