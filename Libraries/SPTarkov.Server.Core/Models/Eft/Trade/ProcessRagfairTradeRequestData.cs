using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Trade;

public record ProcessRagfairTradeRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("offers")]
    public List<OfferRequest>? Offers
    {
        get;
        set;
    }
}
