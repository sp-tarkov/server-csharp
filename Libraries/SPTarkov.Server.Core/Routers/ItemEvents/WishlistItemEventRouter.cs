using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Wishlist;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public class WishlistItemEventRouter(WishlistCallbacks wishlistCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<AddToWishlistRequest>(
            ItemEventActions.ADD_TO_WISHLIST,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(wishlistCallbacks.AddToWishlist(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<RemoveFromWishlistRequest>(
            ItemEventActions.REMOVE_FROM_WISHLIST,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(wishlistCallbacks.RemoveFromWishlist(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<ChangeWishlistItemCategoryRequest>(
            ItemEventActions.CHANGE_WISHLIST_ITEM_CATEGORY,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(wishlistCallbacks.ChangeWishlistItemCategory(pmcData, body, sessionID));
            }
        ),
    ]) { }
