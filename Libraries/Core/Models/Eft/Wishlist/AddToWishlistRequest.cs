using System.Text.Json.Serialization;

namespace Core.Models.Eft.Wishlist;

public record AddToWishlistRequest
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; }

    [JsonPropertyName("items")]
    public Dictionary<string, int>? Items { get; set; }
}
