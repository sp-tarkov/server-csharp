using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Wishlist;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils.Json;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class WishlistController(EventOutputHolder eventOutputHolder)
{
    /// <summary>
    ///     Handle AddToWishList
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request"></param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse AddToWishList(PmcData pmcData, AddToWishlistRequest request, MongoId sessionId)
    {
        pmcData.WishList ??= new DictionaryOrList<MongoId, int>(new Dictionary<MongoId, int>(), []);
        foreach (var item in request.Items)
        {
            pmcData.WishList.Dictionary.Add(item.Key, item.Value);
        }

        return eventOutputHolder.GetOutput(sessionId);
    }

    /// <summary>
    ///     Handle RemoveFromWishList event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request"></param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse RemoveFromWishList(PmcData pmcData, RemoveFromWishlistRequest request, MongoId sessionId)
    {
        foreach (var itemId in request.Items)
        {
            pmcData.WishList.Dictionary.Remove(itemId);
        }

        return eventOutputHolder.GetOutput(sessionId);
    }

    /// <summary>
    ///     Handle changeWishlistItemCategory event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request"></param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse ChangeWishListItemCategory(PmcData pmcData, ChangeWishlistItemCategoryRequest request, MongoId sessionId)
    {
        pmcData.WishList.Dictionary[request.Item] = request.Category.Value;

        return eventOutputHolder.GetOutput(sessionId);
    }
}
