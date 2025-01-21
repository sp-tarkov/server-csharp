using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public record InventoryMoveRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("to")]
    public To? To { get; set; }
}
