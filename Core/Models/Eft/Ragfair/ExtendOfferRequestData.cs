using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ragfair;

public class ExtendOfferRequestData
{
	[JsonPropertyName("offerId")]
	public string OfferId { get; set; }

	[JsonPropertyName("renewalTime")]
	public int RenewalTime { get; set; }
}