using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ragfair;

public class GetRagfairOfferByIdRequest
{
	[JsonPropertyName("id")]
	public int Id { get; set; }
}