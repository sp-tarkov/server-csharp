using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.Inventory;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;
using Core.Models.Enums;
using SptCommon.Annotations;

namespace Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
public class InventoryItemEventRouter : ItemEventRouterDefinition
{
    protected HideoutCallbacks _hideoutCallbacks;
    protected InventoryCallbacks _inventoryCallbacks;

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
        return new List<HandledRoute>
        {
            new(ItemEventActions.MOVE, false),
            new(ItemEventActions.REMOVE, false),
            new(ItemEventActions.SPLIT, false),
            new(ItemEventActions.MERGE, false),
            new(ItemEventActions.TRANSFER, false),
            new(ItemEventActions.SWAP, false),
            new(ItemEventActions.FOLD, false),
            new(ItemEventActions.TOGGLE, false),
            new(ItemEventActions.TAG, false),
            new(ItemEventActions.BIND, false),
            new(ItemEventActions.UNBIND, false),
            new(ItemEventActions.EXAMINE, false),
            new(ItemEventActions.READ_ENCYCLOPEDIA, false),
            new(ItemEventActions.APPLY_INVENTORY_CHANGES, false),
            new(ItemEventActions.CREATE_MAP_MARKER, false),
            new(ItemEventActions.DELETE_MAP_MARKER, false),
            new(ItemEventActions.EDIT_MAP_MARKER, false),
            new(ItemEventActions.OPEN_RANDOM_LOOT_CONTAINER, false),
            new(ItemEventActions.HIDEOUT_QTE_EVENT, false),
            new(ItemEventActions.REDEEM_PROFILE_REWARD, false),
            new(ItemEventActions.SET_FAVORITE_ITEMS, false),
            new(ItemEventActions.QUEST_FAIL, false),
            new(ItemEventActions.PIN_LOCK, false)
        };
    }

    public override ItemEventRouterResponse HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID,
        ItemEventRouterResponse output)
    {
        switch (url)
        {
            case ItemEventActions.MOVE:
                return _inventoryCallbacks.MoveItem(pmcData, body as InventoryMoveRequestData, sessionID, output);
            case ItemEventActions.REMOVE:
                return _inventoryCallbacks.RemoveItem(pmcData, body as InventoryRemoveRequestData, sessionID, output);
            case ItemEventActions.SPLIT:
                return _inventoryCallbacks.SplitItem(pmcData, body as InventorySplitRequestData, sessionID, output);
            case ItemEventActions.MERGE:
                return _inventoryCallbacks.MergeItem(pmcData, body as InventoryMergeRequestData, sessionID, output);
            case ItemEventActions.TRANSFER:
                return _inventoryCallbacks.TransferItem(pmcData, body as InventoryTransferRequestData, sessionID, output);
            case ItemEventActions.SWAP:
                return _inventoryCallbacks.SwapItem(pmcData, body as InventorySwapRequestData, sessionID);
            case ItemEventActions.FOLD:
                return _inventoryCallbacks.FoldItem(pmcData, body as InventoryFoldRequestData, sessionID);
            case ItemEventActions.TOGGLE:
                return _inventoryCallbacks.ToggleItem(pmcData, body as InventoryToggleRequestData, sessionID);
            case ItemEventActions.TAG:
                return _inventoryCallbacks.TagItem(pmcData, body as InventoryTagRequestData, sessionID);
            case ItemEventActions.BIND:
                return _inventoryCallbacks.BindItem(pmcData, body as InventoryBindRequestData, sessionID, output);
            case ItemEventActions.UNBIND:
                return _inventoryCallbacks.UnBindItem(pmcData, body as InventoryBindRequestData, sessionID, output);
            case ItemEventActions.EXAMINE:
                return _inventoryCallbacks.ExamineItem(pmcData, body as InventoryExamineRequestData, sessionID, output);
            case ItemEventActions.READ_ENCYCLOPEDIA:
                return _inventoryCallbacks.ReadEncyclopedia(pmcData, body as InventoryReadEncyclopediaRequestData, sessionID);
            case ItemEventActions.APPLY_INVENTORY_CHANGES:
                return _inventoryCallbacks.SortInventory(pmcData, body as InventorySortRequestData, sessionID, output);
            case ItemEventActions.CREATE_MAP_MARKER:
                return _inventoryCallbacks.CreateMapMarker(pmcData, body as InventoryCreateMarkerRequestData, sessionID, output);
            case ItemEventActions.DELETE_MAP_MARKER:
                return _inventoryCallbacks.DeleteMapMarker(pmcData, body as InventoryDeleteMarkerRequestData, sessionID, output);
            case ItemEventActions.EDIT_MAP_MARKER:
                return _inventoryCallbacks.EditMapMarker(pmcData, body as InventoryEditMarkerRequestData, sessionID, output);
            case ItemEventActions.OPEN_RANDOM_LOOT_CONTAINER:
                return _inventoryCallbacks.OpenRandomLootContainer(pmcData, body as OpenRandomLootContainerRequestData, sessionID, output);
            case ItemEventActions.HIDEOUT_QTE_EVENT:
                return _hideoutCallbacks.HandleQTEEvent(pmcData, body as HandleQTEEventRequestData, sessionID, output);
            case ItemEventActions.REDEEM_PROFILE_REWARD:
                return _inventoryCallbacks.RedeemProfileReward(pmcData, body as RedeemProfileRequestData, sessionID, output);
            case ItemEventActions.SET_FAVORITE_ITEMS:
                return _inventoryCallbacks.SetFavoriteItem(pmcData, body as SetFavoriteItems, sessionID, output);
            case ItemEventActions.QUEST_FAIL:
                return _inventoryCallbacks.FailQuest(pmcData, body as FailQuestRequestData, sessionID, output);
            case ItemEventActions.PIN_LOCK:
                return _inventoryCallbacks.PinOrLock(pmcData, body as PinOrLockItemRequest, sessionID, output);
            default:
                throw new Exception($"InventoryItemEventRouter being used when it cant handle route {url}");
        }
    }
}
