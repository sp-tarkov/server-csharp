using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Trade;

public record SellScavItemsToFenceRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("totalValue")]
    public double? TotalValue { get; set; }
}
