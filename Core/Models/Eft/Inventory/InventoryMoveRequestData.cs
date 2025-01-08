using System.Text.Json.Serialization;
using Core.Models.Eft.ItemEvent;

namespace Core.Models.Eft.Inventory;

public class InventoryMoveRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "Move";

    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("to")]
    public To? To { get; set; }
}