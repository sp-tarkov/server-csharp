using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.Wishlist;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public sealed class WishlistItemEventRouter(WishlistCallbacks wishlistCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<AddToWishlistRequest>(
            ItemEventActions.ADD_TO_WISHLIST,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await wishlistCallbacks.AddToWishlist(pmcData, body, sessionID)
        ),
        new ItemRouteAction<RemoveFromWishlistRequest>(
            ItemEventActions.REMOVE_FROM_WISHLIST,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await wishlistCallbacks.RemoveFromWishlist(pmcData, body, sessionID)
        ),
        new ItemRouteAction<ChangeWishlistItemCategoryRequest>(
            ItemEventActions.CHANGE_WISHLIST_ITEM_CATEGORY,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await wishlistCallbacks.ChangeWishlistItemCategory(pmcData, body, sessionID)
        ),
    ]) { }
