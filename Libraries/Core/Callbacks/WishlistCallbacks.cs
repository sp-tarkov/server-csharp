using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Wishlist;
using SptCommon.Annotations;

namespace Core.Callbacks;

[Injectable]
public class WishlistCallbacks(WishlistController _wishlistController)
{
    /// <summary>
    ///     Handle AddToWishList event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse AddToWishlist(PmcData pmcData, AddToWishlistRequest info, string sessionID)
    {
        return _wishlistController.AddToWishList(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle RemoveFromWishList event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse RemoveFromWishlist(PmcData pmcData, RemoveFromWishlistRequest info, string sessionID)
    {
        return _wishlistController.RemoveFromWishList(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle ChangeWishlistItemCategory
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ChangeWishlistItemCategory(PmcData pmcData, ChangeWishlistItemCategoryRequest info, string sessionID)
    {
        return _wishlistController.ChangeWishListItemCategory(pmcData, info, sessionID);
    }
}
