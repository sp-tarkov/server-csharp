using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public class InventoryToggleRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "Toggle";

    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("value")]
    public bool? Value { get; set; }
}