using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Enums.Hideout;

namespace SPTarkov.Server.Core.Models.Eft.Hideout;

public record HideoutTakeItemOutRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("areaType")]
    public HideoutAreas? AreaType { get; set; }

    [JsonPropertyName("slots")]
    public List<int>? Slots { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
