using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Ragfair;

public record GetOffersResult
{
    [JsonPropertyName("categories")]
    public Dictionary<string, int>? Categories { get; set; }

    [JsonPropertyName("offers")]
    public List<RagfairOffer>? Offers { get; set; }

    [JsonPropertyName("offersCount")]
    public int? OffersCount { get; set; }

    [JsonPropertyName("selectedCategory")]
    public string? SelectedCategory { get; set; }
}
