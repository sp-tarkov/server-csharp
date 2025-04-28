using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Wishlist;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
public class WishlistItemEventRouter : ItemEventRouterDefinition
{
    protected WishlistCallbacks _wishlistCallbacks;

    public WishlistItemEventRouter
    (
        WishlistCallbacks wishlistCallbacks
    )
    {
        _wishlistCallbacks = wishlistCallbacks;
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new List<HandledRoute>
        {
            new(ItemEventActions.ADD_TO_WISHLIST, false),
            new(ItemEventActions.REMOVE_FROM_WISHLIST, false),
            new(ItemEventActions.CHANGE_WISHLIST_ITEM_CATEGORY, false)
        };
    }

    public override ItemEventRouterResponse HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID,
        ItemEventRouterResponse output)
    {
        switch (url)
        {
            case ItemEventActions.ADD_TO_WISHLIST:
                return _wishlistCallbacks.AddToWishlist(pmcData, body as AddToWishlistRequest, sessionID);
            case ItemEventActions.REMOVE_FROM_WISHLIST:
                return _wishlistCallbacks.RemoveFromWishlist(pmcData, body as RemoveFromWishlistRequest, sessionID);
            case ItemEventActions.CHANGE_WISHLIST_ITEM_CATEGORY:
                return _wishlistCallbacks.ChangeWishlistItemCategory(pmcData, body as ChangeWishlistItemCategoryRequest, sessionID);
            default:
                throw new Exception($"CustomizationItemEventRouter being used when it cant handle route {url}");
        }
    }
}
