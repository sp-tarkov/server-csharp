using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryDeleteMarkerRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item
    {
        get;
        set;
    }

    [JsonPropertyName("X")]
    public int? X
    {
        get;
        set;
    }

    [JsonPropertyName("Y")]
    public int? Y
    {
        get;
        set;
    }
}
