using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Eft.Inventory;

public record InventorySortRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "ApplyInventoryChanges";

    [JsonPropertyName("changedItems")]
    public List<Item>? ChangedItems { get; set; }
}
