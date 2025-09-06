using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryCreateMarkerRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public MongoId? Item { get; set; }

    [JsonPropertyName("mapMarker")]
    public MapMarker? MapMarker { get; set; }
}
