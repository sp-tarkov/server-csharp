using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Wishlist;

public record ChangeWishlistItemCategoryRequest : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item
    {
        get;
        set;
    }

    [JsonPropertyName("category")]
    public int? Category
    {
        get;
        set;
    }
}
