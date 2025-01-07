using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public class InventoryDeleteMarkerRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string Action { get; set; } = "DeleteMapMarker";

    [JsonPropertyName("item")]
    public string Item { get; set; }

    [JsonPropertyName("X")]
    public int X { get; set; }

    [JsonPropertyName("Y")]
    public int Y { get; set; }
}