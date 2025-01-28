using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Inventory;
using Core.Models.Enums;

namespace Core.Models.Eft.Hideout;

public record HideoutPutItemInRequestData : InventoryBaseActionRequestData
{

    [JsonPropertyName("areaType")]
    public HideoutAreas? AreaType { get; set; }

    [JsonPropertyName("items")]
    public Dictionary<string, IdWithCount>? Items { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
