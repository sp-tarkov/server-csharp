using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.Inventory;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;
using Core.Models.Enums;

namespace Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
public class InventoryItemEventRouter : ItemEventRouterDefinition
{
    protected InventoryCallbacks _inventoryCallbacks;
    protected HideoutCallbacks _hideoutCallbacks;

    public InventoryItemEventRouter
    (
        InventoryCallbacks inventoryCallbacks,
        HideoutCallbacks hideoutCallbacks
    )
    {
        _inventoryCallbacks = inventoryCallbacks;
        _hideoutCallbacks = hideoutCallbacks;
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new()
        {
            new HandledRoute(ItemEventActions.MOVE, false),
            new HandledRoute(ItemEventActions.REMOVE, false),
            new HandledRoute(ItemEventActions.SPLIT, false),
            new HandledRoute(ItemEventActions.MERGE, false),
            new HandledRoute(ItemEventActions.TRANSFER, false),
            new HandledRoute(ItemEventActions.SWAP, false),
            new HandledRoute(ItemEventActions.FOLD, false),
            new HandledRoute(ItemEventActions.TOGGLE, false),
            new HandledRoute(ItemEventActions.TAG, false),
            new HandledRoute(ItemEventActions.BIND, false),
            new HandledRoute(ItemEventActions.UNBIND, false),
            new HandledRoute(ItemEventActions.EXAMINE, false),
            new HandledRoute(ItemEventActions.READ_ENCYCLOPEDIA, false),
            new HandledRoute(ItemEventActions.APPLY_INVENTORY_CHANGES, false),
            new HandledRoute(ItemEventActions.CREATE_MAP_MARKER, false),
            new HandledRoute(ItemEventActions.DELETE_MAP_MARKER, false),
            new HandledRoute(ItemEventActions.EDIT_MAP_MARKER, false),
            new HandledRoute(ItemEventActions.OPEN_RANDOM_LOOT_CONTAINER, false),
            new HandledRoute(ItemEventActions.HIDEOUT_QTE_EVENT, false),
            new HandledRoute(ItemEventActions.REDEEM_PROFILE_REWARD, false),
            new HandledRoute(ItemEventActions.SET_FAVORITE_ITEMS, false),
            new HandledRoute(ItemEventActions.QUEST_FAIL, false),
            new HandledRoute(ItemEventActions.PIN_LOCK, false)
        };
    }

    public override Task<ItemEventRouterResponse> HandleItemEvent(string url, PmcData pmcData, object body, string sessionID, ItemEventRouterResponse output)
    {
        switch (url) {
            case ItemEventActions.MOVE:
                return Task.FromResult(_inventoryCallbacks.MoveItem(pmcData, body as InventoryMoveRequestData, sessionID, output));
            case ItemEventActions.REMOVE:
                return Task.FromResult(_inventoryCallbacks.RemoveItem(pmcData, body as InventoryRemoveRequestData, sessionID, output));
            case ItemEventActions.SPLIT:
                return Task.FromResult(_inventoryCallbacks.SplitItem(pmcData, body as InventorySplitRequestData, sessionID, output));
            case ItemEventActions.MERGE:
                return Task.FromResult(_inventoryCallbacks.MergeItem(pmcData, body as InventoryMergeRequestData, sessionID, output));
            case ItemEventActions.TRANSFER:
                return Task.FromResult(_inventoryCallbacks.TransferItem(pmcData, body as InventoryTransferRequestData, sessionID, output));
            case ItemEventActions.SWAP:
                return Task.FromResult(_inventoryCallbacks.SwapItem(pmcData, body as InventorySwapRequestData, sessionID));
            case ItemEventActions.FOLD:
                return Task.FromResult(_inventoryCallbacks.FoldItem(pmcData, body as InventoryFoldRequestData, sessionID));
            case ItemEventActions.TOGGLE:
                return Task.FromResult(_inventoryCallbacks.ToggleItem(pmcData, body as InventoryToggleRequestData, sessionID));
            case ItemEventActions.TAG:
                return Task.FromResult(_inventoryCallbacks.TagItem(pmcData, body as InventoryTagRequestData, sessionID));
            case ItemEventActions.BIND:
                return Task.FromResult(_inventoryCallbacks.BindItem(pmcData, body as InventoryBindRequestData, sessionID, output));
            case ItemEventActions.UNBIND:
                return Task.FromResult(_inventoryCallbacks.UnBindItem(pmcData, body as InventoryBindRequestData, sessionID, output));
            case ItemEventActions.EXAMINE:
                return Task.FromResult(_inventoryCallbacks.ExamineItem(pmcData, body as InventoryExamineRequestData, sessionID, output));
            case ItemEventActions.READ_ENCYCLOPEDIA:
                return Task.FromResult(_inventoryCallbacks.ReadEncyclopedia(pmcData, body as InventoryReadEncyclopediaRequestData, sessionID));
            case ItemEventActions.APPLY_INVENTORY_CHANGES:
                return Task.FromResult(_inventoryCallbacks.SortInventory(pmcData, body as InventorySortRequestData, sessionID, output));
            case ItemEventActions.CREATE_MAP_MARKER:
                return Task.FromResult(_inventoryCallbacks.CreateMapMarker(pmcData, body as InventoryCreateMarkerRequestData, sessionID, output));
            case ItemEventActions.DELETE_MAP_MARKER:
                return Task.FromResult(_inventoryCallbacks.DeleteMapMarker(pmcData, body as InventoryDeleteMarkerRequestData, sessionID, output));
            case ItemEventActions.EDIT_MAP_MARKER:
                return Task.FromResult(_inventoryCallbacks.EditMapMarker(pmcData, body as InventoryEditMarkerRequestData, sessionID, output));
            case ItemEventActions.OPEN_RANDOM_LOOT_CONTAINER:
                return Task.FromResult(_inventoryCallbacks.OpenRandomLootContainer(pmcData, body as OpenRandomLootContainerRequestData, sessionID, output));
            case ItemEventActions.HIDEOUT_QTE_EVENT:
                return Task.FromResult(_hideoutCallbacks.HandleQTEEvent(pmcData, body as HandleQTEEventRequestData, sessionID, output));
            case ItemEventActions.REDEEM_PROFILE_REWARD:
                return Task.FromResult(_inventoryCallbacks.RedeemProfileReward(pmcData, body as RedeemProfileRequestData, sessionID, output));
            case ItemEventActions.SET_FAVORITE_ITEMS:
                return Task.FromResult(_inventoryCallbacks.SetFavoriteItem(pmcData, body as SetFavoriteItems, sessionID, output));
            case ItemEventActions.QUEST_FAIL:
                return Task.FromResult(_inventoryCallbacks.FailQuest(pmcData, body as FailQuestRequestData, sessionID, output));
            case ItemEventActions.PIN_LOCK:
                return Task.FromResult(_inventoryCallbacks.PinOrLock(pmcData, body as  PinOrLockItemRequest, sessionID, output));
            default:
                throw new Exception($"InventoryItemEventRouter being used when it cant handle route {url}");
        }
    }
}
