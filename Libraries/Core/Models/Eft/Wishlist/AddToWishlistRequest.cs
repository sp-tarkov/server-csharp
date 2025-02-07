using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Wishlist;

public record AddToWishlistRequest : InventoryBaseActionRequestData
{
    [JsonPropertyName("items")]
    public Dictionary<string, int>? Items
    {
        get;
        set;
    }
}
