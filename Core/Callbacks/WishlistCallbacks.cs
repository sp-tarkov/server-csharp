using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Wishlist;

namespace Core.Callbacks;

public class WishlistCallbacks
{
    public WishlistCallbacks()
    {
        
    }

    /// <summary>
    /// Handle AddToWishList event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse AddToWishlist(PmcData pmcData, AddToWishlistRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle RemoveFromWishList event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse RemoveFromWishlist(PmcData pmcData, RemoveFromWishlistRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle ChangeWishlistItemCategory
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse ChangeWishlistItemCategory(PmcData pmcData, ChangeWishlistItemCategoryRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }
}