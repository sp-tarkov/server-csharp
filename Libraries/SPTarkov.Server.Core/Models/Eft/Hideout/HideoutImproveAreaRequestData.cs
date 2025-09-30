using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Enums.Hideout;

namespace SPTarkov.Server.Core.Models.Eft.Hideout;

public record HideoutImproveAreaRequestData : InventoryBaseActionRequestData
{
    /// <summary>
    ///     Hideout area id from areas.json
    /// </summary>
    [JsonPropertyName("id")]
    public MongoId AreaId { get; set; }

    [JsonPropertyName("areaType")]
    public HideoutAreas? AreaType { get; set; }

    [JsonPropertyName("items")]
    public List<HideoutItem>? Items { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
