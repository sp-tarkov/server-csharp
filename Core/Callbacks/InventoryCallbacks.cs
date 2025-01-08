using Core.Models.Eft.Common;
using Core.Models.Eft.Inventory;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;

namespace Core.Callbacks;

public class InventoryCallbacks
{
    public InventoryCallbacks()
    {
    }

    /// <summary>
    /// Handle client/game/profile/items/moving Move event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse MoveItem(PmcData pmcData, InventoryMoveRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle Remove event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse RemoveItem(PmcData pmcData, InventoryRemoveRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle Split event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse SplitItem(PmcData pmcData, InventorySplitRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse MergeItem(PmcData pmcData, InventoryMergeRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse TransferItem(PmcData pmcData, InventoryTransferRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle Swap
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse SwapItem(PmcData pmcData, InventorySwapRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse FoldItem(PmcData pmcData, InventoryFoldRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse ToggleItem(PmcData pmcData, InventoryToggleRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse TagItem(PmcData pmcData, InventoryTagRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse BindItem(PmcData pmcData, InventoryBindRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse UnBindItem(PmcData pmcData, InventoryBindRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse ExamineItem(PmcData pmcData, InventoryExamineRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle ReadEncyclopedia
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse ReadEncyclopedia(PmcData pmcData, InventoryReadEncyclopediaRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle ApplyInventoryChanges
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse SortInventory(PmcData pmcData, InventorySortRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse CreateMapMarker(PmcData pmcData, InventoryCreateMarkerRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse DeleteMapMarker(PmcData pmcData, InventoryDeleteMarkerRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse EditMapMarker(PmcData pmcData, InventoryEditMarkerRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle OpenRandomLootContainer
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse OpenRandomLootContainer(PmcData pmcData, OpenRandomLootContainerRequestData info, string sessionID,
        ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse RedeemProfileReward(PmcData pmcData, RedeemProfileRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse SetFavoriteItem(PmcData pmcData, SetFavoriteItems info, string sessionID, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// TODO: MOVE INTO QUEST CODE
    /// Handle game/profile/items/moving - QuestFail
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse FailQuest(PmcData pmcData, FailQuestRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse PinOrLock(PmcData pmcData, PinOrLockItemRequest info, string sessionID, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }
}