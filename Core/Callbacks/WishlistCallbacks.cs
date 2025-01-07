using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Wishlist;

namespace Core.Callbacks;

public class WishlistCallbacks
{
    public WishlistCallbacks()
    {
        
    }

    public ItemEventRouterResponse AddToWishlist(PmcData pmcData, AddToWishlistRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse RemoveFromWishlist(PmcData pmcData, RemoveFromWishlistRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse ChangeWishlistItemCategory(PmcData pmcData, ChangeWishlistItemCategoryRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }
}