using System.Text.Json.Serialization;

namespace Core.Models.Eft.Wishlist;

public class ChangeWishlistItemCategoryRequest
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; }

    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("category")]
    public int? Category { get; set; }
}