using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Wishlist;

public record RemoveFromWishlistRequest : InventoryBaseActionRequestData
{
    [JsonPropertyName("items")]
    public List<string>? Items
    {
        get;
        set;
    }
}
