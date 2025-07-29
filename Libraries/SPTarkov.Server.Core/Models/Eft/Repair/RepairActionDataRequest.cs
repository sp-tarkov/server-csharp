using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Repair;

public record RepairActionDataRequest : InventoryBaseActionRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    [JsonPropertyName("repairKitsInfo")]
    public List<RepairKitsInfo>? RepairKitsInfo { get; set; }

    /// <summary>
    ///     item to repair
    /// </summary>
    [JsonPropertyName("target")]
    public MongoId? Target { get; set; }
}

public record RepairKitsInfo
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    /// <summary>
    ///     id of repair kit to use
    /// </summary>
    [JsonPropertyName("_id")]
    public MongoId Id { get; set; }

    /// <summary>
    ///     amount of units to reduce kit by
    /// </summary>
    [JsonPropertyName("count")]
    public float? Count { get; set; }
}
