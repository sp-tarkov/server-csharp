using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Wishlist;
using Core.Routers;
using SptCommon.Annotations;

namespace Core.Controllers;

[Injectable]
public class WishlistController(
    EventOutputHolder _eventOutputHolder
)
{
    /// <summary>
    ///     Handle AddToWishList
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="request"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public ItemEventRouterResponse AddToWishList(
        PmcData pmcData,
        AddToWishlistRequest request,
        string sessionId)
    {
        foreach (var item in request.Items)
        {
            pmcData.WishList.Dictionary.Add(item.Key, item.Value);
        }

        return _eventOutputHolder.GetOutput(sessionId);
    }

    /// <summary>
    ///     Handle RemoveFromWishList event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="request"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public ItemEventRouterResponse RemoveFromWishList(
        PmcData pmcData,
        RemoveFromWishlistRequest request,
        string sessionId)
    {
        foreach (var itemId in request.Items)
        {
            pmcData.WishList.Dictionary.Remove(itemId);
        }

        return _eventOutputHolder.GetOutput(sessionId);
    }

    /// <summary>
    ///     Handle changeWishlistItemCategory event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="request"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ChangeWishListItemCategory(
        PmcData pmcData,
        ChangeWishlistItemCategoryRequest request,
        string sessionId)
    {
        pmcData.WishList.Dictionary[request.Item] = request.Category.Value;

        return _eventOutputHolder.GetOutput(sessionId);
    }
}
