using System.Text.Json.Serialization;

namespace Core.Models.Eft.Trade;

public record ProcessBuyTradeRequestData : ProcessBaseTradeRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } // TODO: formerly - "buy_from_trader" | "TradingConfirm" | "RestoreHealth" | "SptInsure" | "SptRepair" | ""

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("tid")]
    public string? TId { get; set; }

    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("scheme_id")]
    public int? SchemeId { get; set; }

    [JsonPropertyName("scheme_items")]
    public List<ItemRequest>? SchemeItems { get; set; }
}
