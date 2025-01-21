using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Repair;

public record RepairActionDataRequest : InventoryBaseActionRequestData
{
    [JsonPropertyName("repairKitsInfo")]
    public List<RepairKitsInfo>? RepairKitsInfo { get; set; }

    [JsonPropertyName("target")]
    public string? Target { get; set; } // item to repair
}

public record RepairKitsInfo
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; } // id of repair kit to use

    [JsonPropertyName("count")]
    public int? Count { get; set; } // amount of units to reduce kit by
}
