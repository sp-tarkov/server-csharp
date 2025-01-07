using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public class InventoryRemoveRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string Action { get; set; } = "Remove";

    [JsonPropertyName("item")]
    public string Item { get; set; }
}