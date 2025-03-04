using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Inventory;
using Core.Models.Enums;

namespace Core.Models.Eft.Hideout;

public record HideoutImproveAreaRequestData : InventoryBaseActionRequestData
{
    /// <summary>
    /// Hideout area id from areas.json
    /// </summary>
    [JsonPropertyName("id")]
    public string? AreaId
    {
        get;
        set;
    }

    [JsonPropertyName("areaType")]
    public HideoutAreas? AreaType
    {
        get;
        set;
    }

    [JsonPropertyName("items")]
    public List<HideoutItem>? Items
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
