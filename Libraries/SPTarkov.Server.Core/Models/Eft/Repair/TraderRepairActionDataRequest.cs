using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Repair;

public record TraderRepairActionDataRequest : InventoryBaseActionRequestData
{
    [JsonPropertyName("tid")]
    public MongoId TraderId { get; set; }

    [JsonPropertyName("repairItems")]
    public List<RepairItem>? RepairItems { get; set; }
}

public record RepairItem
{


    [JsonPropertyName("_id")]
    public MongoId Id { get; set; }

    [JsonPropertyName("count")]
    public double? Count { get; set; }
}
