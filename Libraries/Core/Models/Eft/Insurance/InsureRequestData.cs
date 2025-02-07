using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Insurance;

public record InsureRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("tid")]
    public string? TransactionId
    {
        get;
        set;
    }

    [JsonPropertyName("items")]
    public List<string>? Items
    {
        get;
        set;
    }
}
