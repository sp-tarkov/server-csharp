using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ragfair;

public class RemoveOfferRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; }

    [JsonPropertyName("offerId")]
    public string? OfferId { get; set; }
}