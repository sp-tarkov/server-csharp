using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ragfair;

public record ExtendOfferRequestData
{
    [JsonPropertyName("offerId")]
    public string? OfferId { get; set; }

    [JsonPropertyName("renewalTime")]
    public int? RenewalTime { get; set; }
}
