using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public record InventoryUnbindRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item
    {
        get;
        set;
    }

    [JsonPropertyName("index")]
    public int? Index
    {
        get;
        set;
    }
}
