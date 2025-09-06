using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Ragfair;

public record RemoveOfferRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("offerId")]
    public string? OfferId { get; set; }
}
