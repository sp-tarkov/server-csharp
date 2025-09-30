using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Enums.Hideout;

namespace SPTarkov.Server.Core.Models.Eft.Hideout;

public record HideoutPutItemInRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("areaType")]
    public HideoutAreas? AreaType { get; set; }

    [JsonPropertyName("items")]
    public Dictionary<string, IdWithCount>? Items { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
