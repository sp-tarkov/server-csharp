using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public class InventoryBindRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string Action { get; set; } = "Bind";

    [JsonPropertyName("item")]
    public string Item { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }
}