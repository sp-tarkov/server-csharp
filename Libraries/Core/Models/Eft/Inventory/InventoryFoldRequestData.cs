using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public record InventoryFoldRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "Fold";

    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("value")]
    public bool? Value { get; set; }
}
