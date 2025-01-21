using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public record InventorySplitRequestData : InventoryBaseActionRequestData
{
    /** Id of item to split */
    [JsonPropertyName("splitItem")]
    public string? SplitItem { get; set; }

    /** Id of new item stack */
    [JsonPropertyName("newItem")]
    public string? NewItem { get; set; }

    /** Destination new item will be placed in */
    [JsonPropertyName("container")]
    public Container? Container { get; set; }

    public int? Count { get; set; }
}
