using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Ragfair;

public record GetOffersResult
{
    [JsonPropertyName("categories")]
    public Dictionary<MongoId, int>? Categories
    {
        get;
        set;
    }

    [JsonPropertyName("offers")]
    public List<RagfairOffer>? Offers
    {
        get;
        set;
    }

    [JsonPropertyName("offersCount")]
    public int? OffersCount
    {
        get;
        set;
    }

    [JsonPropertyName("selectedCategory")]
    public string? SelectedCategory
    {
        get;
        set;
    }
}
