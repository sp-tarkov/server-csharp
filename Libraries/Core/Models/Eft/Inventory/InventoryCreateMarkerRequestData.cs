using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Eft.Inventory;

public record InventoryCreateMarkerRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("mapMarker")]
    public MapMarker? MapMarker { get; set; }
}
