using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Wishlist;

public record RemoveFromWishlistRequest : InventoryBaseActionRequestData
{
    [JsonPropertyName("items")]
    public List<string>? Items
    {
        get;
        set;
    }
}
