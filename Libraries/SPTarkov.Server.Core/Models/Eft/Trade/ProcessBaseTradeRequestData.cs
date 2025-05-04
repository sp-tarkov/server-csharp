using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Trade;

public record ProcessBaseTradeRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("tid")]
    public string? TransactionId { get; set; }
}
