using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Eft.Inventory;

public record InventoryEditMarkerRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item
    {
        get;
        set;
    }

    [JsonPropertyName("X")]
    public double? X
    {
        get;
        set;
    }

    [JsonPropertyName("Y")]
    public double? Y
    {
        get;
        set;
    }

    [JsonPropertyName("mapMarker")]
    public MapMarker? MapMarker
    {
        get;
        set;
    }
}
