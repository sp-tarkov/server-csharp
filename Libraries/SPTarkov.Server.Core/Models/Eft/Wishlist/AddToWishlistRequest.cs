using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Wishlist;

public record AddToWishlistRequest : InventoryBaseActionRequestData
{
    [JsonPropertyName("items")]
    public Dictionary<MongoId, int>? Items { get; set; }
}
