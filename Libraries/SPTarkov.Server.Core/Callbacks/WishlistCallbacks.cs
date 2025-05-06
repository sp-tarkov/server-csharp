using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Wishlist;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class WishlistCallbacks(WishlistController _wishlistController)
{
    /// <summary>
    ///     Handle AddToWishList event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse AddToWishlist(PmcData pmcData, AddToWishlistRequest info, string sessionID)
    {
        return _wishlistController.AddToWishList(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle RemoveFromWishList event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse RemoveFromWishlist(PmcData pmcData, RemoveFromWishlistRequest info, string sessionID)
    {
        return _wishlistController.RemoveFromWishList(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle ChangeWishlistItemCategory
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse ChangeWishlistItemCategory(PmcData pmcData, ChangeWishlistItemCategoryRequest info, string sessionID)
    {
        return _wishlistController.ChangeWishListItemCategory(pmcData, info, sessionID);
    }
}
