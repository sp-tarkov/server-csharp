using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public record InventoryMergeRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "Merge";

    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("with")]
    public string? With { get; set; }
}
