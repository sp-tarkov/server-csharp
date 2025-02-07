using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public record InventoryExamineRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item
    {
        get;
        set;
    }
}
