using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public record InventoryBindRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("index")]
    public string? Index { get; set; }
}
