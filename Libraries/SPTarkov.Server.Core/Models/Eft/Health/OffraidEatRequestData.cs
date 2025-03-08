using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Health;

public record OffraidEatRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item
    {
        get;
        set;
    }

    [JsonPropertyName("count")]
    public int? Count
    {
        get;
        set;
    }

    [JsonPropertyName("time")]
    public long? Time
    {
        get;
        set;
    }
}
