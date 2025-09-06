using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryEditMarkerRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public MongoId? Item { get; set; }

    [JsonPropertyName("X")]
    public double? X { get; set; }

    [JsonPropertyName("Y")]
    public double? Y { get; set; }

    [JsonPropertyName("mapMarker")]
    public MapMarker? MapMarker { get; set; }
}
