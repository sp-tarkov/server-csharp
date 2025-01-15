using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

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
