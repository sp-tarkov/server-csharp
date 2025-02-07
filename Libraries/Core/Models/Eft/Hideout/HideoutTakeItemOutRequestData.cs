using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;
using Core.Models.Enums;

namespace Core.Models.Eft.Hideout;

public record HideoutTakeItemOutRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("areaType")]
    public HideoutAreas? AreaType
    {
        get;
        set;
    }

    [JsonPropertyName("slots")]
    public List<int>? Slots
    {
        get;
        set;
    }

    [JsonPropertyName("timestamp")]
    public long? Timestamp
    {
        get;
        set;
    }
}
