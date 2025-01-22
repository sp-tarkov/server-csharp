using System.Text.Json.Serialization;

namespace Core.Models.Eft.Trade;

public record ProcessBuyTradeRequestData : ProcessBaseTradeRequestData
{
    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("scheme_id")]
    public int? SchemeId { get; set; }

    [JsonPropertyName("scheme_items")]
    public List<ItemRequest>? SchemeItems { get; set; }
}
