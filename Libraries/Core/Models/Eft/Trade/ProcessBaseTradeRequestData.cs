using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Trade;

public record ProcessBaseTradeRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("type")]
    public string? Type
    {
        get;
        set;
    }

    [JsonPropertyName("tid")]
    public string? TransactionId
    {
        get;
        set;
    }
}
