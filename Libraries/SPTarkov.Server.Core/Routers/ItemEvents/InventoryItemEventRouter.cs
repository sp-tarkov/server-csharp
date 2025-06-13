using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable]
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

    public override ValueTask<ItemEventRouterResponse> HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID,
        ItemEventRouterResponse output)
    {
        switch (url)
        {
            case ItemEventActions.MOVE:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.MoveItem(pmcData, body as InventoryMoveRequestData, sessionID, output));
            case ItemEventActions.REMOVE:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.RemoveItem(pmcData, body as InventoryRemoveRequestData, sessionID, output));
            case ItemEventActions.SPLIT:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.SplitItem(pmcData, body as InventorySplitRequestData, sessionID, output));
            case ItemEventActions.MERGE:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.MergeItem(pmcData, body as InventoryMergeRequestData, sessionID, output));
            case ItemEventActions.TRANSFER:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.TransferItem(pmcData, body as InventoryTransferRequestData, sessionID, output));
            case ItemEventActions.SWAP:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.SwapItem(pmcData, body as InventorySwapRequestData, sessionID));
            case ItemEventActions.FOLD:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.FoldItem(pmcData, body as InventoryFoldRequestData, sessionID));
            case ItemEventActions.TOGGLE:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.ToggleItem(pmcData, body as InventoryToggleRequestData, sessionID));
            case ItemEventActions.TAG:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.TagItem(pmcData, body as InventoryTagRequestData, sessionID));
            case ItemEventActions.BIND:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.BindItem(pmcData, body as InventoryBindRequestData, sessionID, output));
            case ItemEventActions.UNBIND:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.UnBindItem(pmcData, body as InventoryBindRequestData, sessionID, output));
            case ItemEventActions.EXAMINE:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.ExamineItem(pmcData, body as InventoryExamineRequestData, sessionID, output));
            case ItemEventActions.READ_ENCYCLOPEDIA:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.ReadEncyclopedia(pmcData, body as InventoryReadEncyclopediaRequestData, sessionID));
            case ItemEventActions.APPLY_INVENTORY_CHANGES:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.SortInventory(pmcData, body as InventorySortRequestData, sessionID, output));
            case ItemEventActions.CREATE_MAP_MARKER:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.CreateMapMarker(pmcData, body as InventoryCreateMarkerRequestData, sessionID, output));
            case ItemEventActions.DELETE_MAP_MARKER:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.DeleteMapMarker(pmcData, body as InventoryDeleteMarkerRequestData, sessionID, output));
            case ItemEventActions.EDIT_MAP_MARKER:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.EditMapMarker(pmcData, body as InventoryEditMarkerRequestData, sessionID, output));
            case ItemEventActions.OPEN_RANDOM_LOOT_CONTAINER:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.OpenRandomLootContainer(pmcData, body as OpenRandomLootContainerRequestData, sessionID, output));
            case ItemEventActions.HIDEOUT_QTE_EVENT:
                return new ValueTask<ItemEventRouterResponse>(_hideoutCallbacks.HandleQTEEvent(pmcData, body as HandleQTEEventRequestData, sessionID, output));
            case ItemEventActions.REDEEM_PROFILE_REWARD:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.RedeemProfileReward(pmcData, body as RedeemProfileRequestData, sessionID, output));
            case ItemEventActions.SET_FAVORITE_ITEMS:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.SetFavoriteItem(pmcData, body as SetFavoriteItems, sessionID, output));
            case ItemEventActions.QUEST_FAIL:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.FailQuest(pmcData, body as FailQuestRequestData, sessionID, output));
            case ItemEventActions.PIN_LOCK:
                return new ValueTask<ItemEventRouterResponse>(_inventoryCallbacks.PinOrLock(pmcData, body as PinOrLockItemRequest, sessionID, output));
            default:
                throw new Exception($"InventoryItemEventRouter being used when it cant handle route {url}");
        }
    }
}
