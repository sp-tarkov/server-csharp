using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Ragfair;

public record ExtendOfferRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("offerId")]
    public string? OfferId { get; set; }

    [JsonPropertyName("renewalTime")]
    public int? RenewalTime { get; set; }
}
