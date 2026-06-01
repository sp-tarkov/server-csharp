using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public class InventoryItemEventRouter(InventoryCallbacks inventoryCallbacks, HideoutCallbacks hideoutCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<InventoryMoveRequestData>(
            ItemEventActions.MOVE,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.MoveItem(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<InventoryRemoveRequestData>(
            ItemEventActions.REMOVE,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.RemoveItem(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<InventorySplitRequestData>(
            ItemEventActions.SPLIT,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.SplitItem(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<InventoryMergeRequestData>(
            ItemEventActions.MERGE,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.MergeItem(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<InventoryTransferRequestData>(
            ItemEventActions.TRANSFER,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.TransferItem(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<InventorySwapRequestData>(
            ItemEventActions.SWAP,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.SwapItem(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<InventoryFoldRequestData>(
            ItemEventActions.FOLD,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.FoldItem(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<InventoryToggleRequestData>(
            ItemEventActions.TOGGLE,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.ToggleItem(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<InventoryTagRequestData>(
            ItemEventActions.TAG,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.TagItem(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<InventoryBindRequestData>(
            ItemEventActions.BIND,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.BindItem(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<InventoryBindRequestData>(
            ItemEventActions.UNBIND,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.UnBindItem(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<InventoryExamineRequestData>(
            ItemEventActions.EXAMINE,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.ExamineItem(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<InventoryReadEncyclopediaRequestData>(
            ItemEventActions.READ_ENCYCLOPEDIA,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.ReadEncyclopedia(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<InventorySortRequestData>(
            ItemEventActions.APPLY_INVENTORY_CHANGES,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.SortInventory(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<InventoryCreateMarkerRequestData>(
            ItemEventActions.CREATE_MAP_MARKER,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.CreateMapMarker(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<InventoryDeleteMarkerRequestData>(
            ItemEventActions.DELETE_MAP_MARKER,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.DeleteMapMarker(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<InventoryEditMarkerRequestData>(
            ItemEventActions.EDIT_MAP_MARKER,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.EditMapMarker(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<OpenRandomLootContainerRequestData>(
            ItemEventActions.OPEN_RANDOM_LOOT_CONTAINER,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.OpenRandomLootContainer(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<HandleQTEEventRequestData>(
            ItemEventActions.HIDEOUT_QTE_EVENT,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.HandleQTEEvent(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<RedeemProfileRequestData>(
            ItemEventActions.REDEEM_PROFILE_REWARD,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.RedeemProfileReward(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<SetFavoriteItems>(
            ItemEventActions.SET_FAVORITE_ITEMS,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.SetFavoriteItem(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<FailQuestRequestData>(
            ItemEventActions.QUEST_FAIL,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.FailQuest(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<PinOrLockItemRequest>(
            ItemEventActions.PIN_LOCK,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.PinOrLock(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<SaveDialogueStateRequest>(
            ItemEventActions.SAVE_DIALOGUE_STATE,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(inventoryCallbacks.SaveDialogueState(pmcData, body, sessionID, output));
            }
        ),
    ]) { }
