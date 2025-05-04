using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventorySortRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("changedItems")]
    public List<Item>? ChangedItems { get; set; }
}
