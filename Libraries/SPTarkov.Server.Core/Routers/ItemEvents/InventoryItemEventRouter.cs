using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public sealed class InventoryItemEventRouter(InventoryCallbacks inventoryCallbacks, HideoutCallbacks hideoutCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<InventoryMoveRequestData>(
            ItemEventActions.MOVE,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.MoveItem(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<InventoryRemoveRequestData>(
            ItemEventActions.REMOVE,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.RemoveItem(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<InventorySplitRequestData>(
            ItemEventActions.SPLIT,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.SplitItem(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<InventoryMergeRequestData>(
            ItemEventActions.MERGE,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.MergeItem(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<InventoryTransferRequestData>(
            ItemEventActions.TRANSFER,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.TransferItem(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<InventorySwapRequestData>(
            ItemEventActions.SWAP,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await inventoryCallbacks.SwapItem(pmcData, body, sessionID)
        ),
        new ItemRouteAction<InventoryFoldRequestData>(
            ItemEventActions.FOLD,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await inventoryCallbacks.FoldItem(pmcData, body, sessionID)
        ),
        new ItemRouteAction<InventoryToggleRequestData>(
            ItemEventActions.TOGGLE,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.ToggleItem(pmcData, body, sessionID)
        ),
        new ItemRouteAction<InventoryTagRequestData>(
            ItemEventActions.TAG,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await inventoryCallbacks.TagItem(pmcData, body, sessionID)
        ),
        new ItemRouteAction<InventoryBindRequestData>(
            ItemEventActions.BIND,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.BindItem(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<InventoryBindRequestData>(
            ItemEventActions.UNBIND,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.UnBindItem(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<InventoryExamineRequestData>(
            ItemEventActions.EXAMINE,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.ExamineItem(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<InventoryReadEncyclopediaRequestData>(
            ItemEventActions.READ_ENCYCLOPEDIA,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.ReadEncyclopedia(pmcData, body, sessionID)
        ),
        new ItemRouteAction<InventorySortRequestData>(
            ItemEventActions.APPLY_INVENTORY_CHANGES,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.SortInventory(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<InventoryCreateMarkerRequestData>(
            ItemEventActions.CREATE_MAP_MARKER,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.CreateMapMarker(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<InventoryDeleteMarkerRequestData>(
            ItemEventActions.DELETE_MAP_MARKER,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.DeleteMapMarker(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<InventoryEditMarkerRequestData>(
            ItemEventActions.EDIT_MAP_MARKER,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.EditMapMarker(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<OpenRandomLootContainerRequestData>(
            ItemEventActions.OPEN_RANDOM_LOOT_CONTAINER,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.OpenRandomLootContainer(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<HandleQTEEventRequestData>(
            ItemEventActions.HIDEOUT_QTE_EVENT,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.HandleQTEEvent(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<RedeemProfileRequestData>(
            ItemEventActions.REDEEM_PROFILE_REWARD,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.RedeemProfileReward(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<SetFavoriteItems>(
            ItemEventActions.SET_FAVORITE_ITEMS,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.SetFavoriteItem(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<FailQuestRequestData>(
            ItemEventActions.QUEST_FAIL,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.FailQuest(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<PinOrLockItemRequest>(
            ItemEventActions.PIN_LOCK,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.PinOrLock(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<SaveDialogueStateRequest>(
            ItemEventActions.SAVE_DIALOGUE_STATE,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await inventoryCallbacks.SaveDialogueState(pmcData, body, sessionID, output)
        ),
    ]) { }
