using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Repair;

public record TraderRepairActionDataRequest : InventoryBaseActionRequestData
{
    [JsonPropertyName("tid")]
    public string? TId { get; set; }

    [JsonPropertyName("repairItems")]
    public List<RepairItem>? RepairItems { get; set; }
}

public record RepairItem
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("count")]
    public double? Count { get; set; }
}
