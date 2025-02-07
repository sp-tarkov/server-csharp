using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Ragfair;

public record RemoveOfferRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("offerId")]
    public string? OfferId
    {
        get;
        set;
    }
}
