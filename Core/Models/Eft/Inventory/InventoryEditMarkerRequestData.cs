using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public class InventoryEditMarkerRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string Action { get; set; } = "EditMapMarker";

    [JsonPropertyName("item")]
    public string Item { get; set; }

    [JsonPropertyName("X")]
    public double X { get; set; }

    [JsonPropertyName("Y")]
    public double Y { get; set; }

    [JsonPropertyName("mapMarker")]
    public MapMarker MapMarker { get; set; }
}