using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public record InventoryAddRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item
    {
        get;
        set;
    }

    [JsonPropertyName("container")]
    public Container? Container
    {
        get;
        set;
    }
}
