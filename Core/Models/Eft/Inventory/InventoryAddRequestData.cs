using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public class InventoryAddRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "Add";

    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("container")]
    public Container? Container { get; set; }
}
