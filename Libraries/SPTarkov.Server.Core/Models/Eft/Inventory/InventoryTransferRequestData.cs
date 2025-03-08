using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryTransferRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item
    {
        get;
        set;
    }

    [JsonPropertyName("with")]
    public string? With
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
}
