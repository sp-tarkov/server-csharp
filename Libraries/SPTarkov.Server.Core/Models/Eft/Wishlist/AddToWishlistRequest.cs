using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Wishlist;

public record AddToWishlistRequest : InventoryBaseActionRequestData
{
    [JsonPropertyName("items")]
    public Dictionary<string, int>? Items { get; set; }
}
