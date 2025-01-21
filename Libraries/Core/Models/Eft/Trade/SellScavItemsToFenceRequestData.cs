using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Trade;

public record SellScavItemsToFenceRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("totalValue")]
    public double? TotalValue { get; set; }
}
