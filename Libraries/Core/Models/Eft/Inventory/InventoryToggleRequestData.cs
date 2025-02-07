using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public record InventoryToggleRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item
    {
        get;
        set;
    }

    [JsonPropertyName("value")]
    public bool? Value
    {
        get;
        set;
    }
}
