using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;
using Core.Models.Enums;

namespace Core.Models.Eft.Hideout;

public record HideoutToggleAreaRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("areaType")]
    public HideoutAreas? AreaType
    {
        get;
        set;
    }

    [JsonPropertyName("enabled")]
    public bool? Enabled
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
