using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryRemoveRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item
    {
        get;
        set;
    }
}
