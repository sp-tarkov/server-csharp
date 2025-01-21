using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Wishlist;

namespace Core.Routers.ItemEvents;

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
        return new()
        {
            new HandledRoute("AddToWishList", false),
            new HandledRoute("RemoveFromWishList", false),
            new HandledRoute("ChangeWishlistItemCategory", false)
        };
    }

    public override ItemEventRouterResponse HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID, ItemEventRouterResponse output)
    {
        switch (url)
        {
            case "AddToWishList":
                return _wishlistCallbacks.AddToWishlist(pmcData, body as AddToWishlistRequest, sessionID);
            case "RemoveFromWishList":
                return _wishlistCallbacks.RemoveFromWishlist(pmcData, body as RemoveFromWishlistRequest, sessionID);
            case "ChangeWishlistItemCategory":
                return _wishlistCallbacks.ChangeWishlistItemCategory(pmcData, body as ChangeWishlistItemCategoryRequest, sessionID);
            default:
                throw new Exception($"CustomizationItemEventRouter being used when it cant handle route {url}");
        }
    }
}
