using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public class InventoryCreateMarkerRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "CreateMapMarker";

    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("mapMarker")]
    public MapMarker? MapMarker { get; set; }
}

public class MapMarker
{
    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    [JsonPropertyName("X")]
    public double? X { get; set; }

    [JsonPropertyName("Y")]
    public double? Y { get; set; }

    [JsonPropertyName("Note")]
    public string? Note { get; set; }
}