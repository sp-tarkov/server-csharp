using System.Text.Json.Serialization;

namespace Core.Models.Eft.Wishlist;

public class RemoveFromWishlistRequest
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; }

    [JsonPropertyName("items")]
    public List<string>? Items { get; set; }
}