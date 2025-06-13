using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Repair;

public record TraderRepairActionDataRequest : InventoryBaseActionRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("tid")]
    public string? TId
    {
        get;
        set;
    }

    [JsonPropertyName("repairItems")]
    public List<RepairItem>? RepairItems
    {
        get;
        set;
    }
}

public record RepairItem
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("_id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("count")]
    public double? Count
    {
        get;
        set;
    }
}
