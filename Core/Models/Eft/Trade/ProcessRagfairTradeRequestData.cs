using System.Text.Json.Serialization;

namespace Core.Models.Eft.Trade;

public class ProcessRagfairTradeRequestData
{
	[JsonPropertyName("Action")]
	public string Action { get; set; }

	[JsonPropertyName("offers")]
	public List<OfferRequest> Offers { get; set; }
}

public class OfferRequest
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("count")]
	public int Count { get; set; }

	[JsonPropertyName("items")]
	public List<ItemRequest> Items { get; set; }
}

public class ItemRequest
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("count")]
	public int Count { get; set; }
}