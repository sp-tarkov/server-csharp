using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public record InventoryTagRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("TagName")]
    public string? TagName { get; set; }

    [JsonPropertyName("TagColor")]
    public int? TagColor { get; set; }
}
