using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public class AddItemRequestData
{
    // Trader id
    [JsonPropertyName("tid")]
    public string? TraderId { get; set; }

    [JsonPropertyName("items")]
    public List<ItemToAdd>? Items { get; set; }
}

public class ItemToAdd
{
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("sptIsPreset")]
    public bool? IsPreset { get; set; }

    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }
}
