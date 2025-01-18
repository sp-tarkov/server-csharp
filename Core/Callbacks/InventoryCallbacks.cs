using Core.Annotations;
using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Inventory;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;

namespace Core.Callbacks;

[Injectable]
public class InventoryCallbacks(
    InventoryController _inventoryController,
    QuestController _questController
)
{
    /// <summary>
    /// Handle client/game/profile/items/moving Move event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse MoveItem(PmcData pmcData, InventoryMoveRequestData info, string sessionID, ItemEventRouterResponse output)
    {
         _inventoryController.MoveItem(pmcData, info, sessionID, output);

         return output;
    }

    /// <summary>
    /// Handle Remove event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse RemoveItem(PmcData pmcData, InventoryRemoveRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.RemoveItem(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle Split event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse SplitItem(PmcData pmcData, InventorySplitRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.SplitItem(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse MergeItem(PmcData pmcData, InventoryMergeRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.MergeItem(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse TransferItem(PmcData pmcData, InventoryTransferRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.TransferItem(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle Swap
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse SwapItem(PmcData pmcData, InventorySwapRequestData info, string sessionID)
    {
        // _inventoryController.SwapItem(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse FoldItem(PmcData pmcData, InventoryFoldRequestData info, string sessionID)
    {
        // _inventoryController.FoldItem(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ToggleItem(PmcData pmcData, InventoryToggleRequestData info, string sessionID)
    {
        // _inventoryController.ToggleItem(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse TagItem(PmcData pmcData, InventoryTagRequestData info, string sessionID)
    {
        // _inventoryController.TagItem(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse BindItem(PmcData pmcData, InventoryBindRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.BindItem(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse UnBindItem(PmcData pmcData, InventoryBindRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.UnBindItem(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ExamineItem(PmcData pmcData, InventoryExamineRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.ExamineItem(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle ReadEncyclopedia
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ReadEncyclopedia(PmcData pmcData, InventoryReadEncyclopediaRequestData info, string sessionID)
    {
        // _inventoryController.ReadEncyclopedia(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle ApplyInventoryChanges
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse SortInventory(PmcData pmcData, InventorySortRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.SortInventory(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse CreateMapMarker(PmcData pmcData, InventoryCreateMarkerRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.CreateMapMarker(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse DeleteMapMarker(PmcData pmcData, InventoryDeleteMarkerRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.DeleteMapMarker(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse EditMapMarker(PmcData pmcData, InventoryEditMarkerRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.EditMapMarker(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
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
    public ItemEventRouterResponse OpenRandomLootContainer(PmcData pmcData, OpenRandomLootContainerRequestData info, string sessionID,
        ItemEventRouterResponse output)
    {
        // _inventoryController.OpenRandomLootContainer(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse RedeemProfileReward(PmcData pmcData, RedeemProfileRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.RedeemProfileReward(pmcData, info, sessionID);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse SetFavoriteItem(PmcData pmcData, SetFavoriteItems info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.SetFavoriteItem(pmcData, info, sessionID);
        // TODO: InventoryController is not implemented rn
        // return output;
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
    public ItemEventRouterResponse FailQuest(PmcData pmcData, FailQuestRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.FailQuest(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse PinOrLock(PmcData pmcData, PinOrLockItemRequest info, string sessionID, ItemEventRouterResponse output)
    {
        // _inventoryController.PinOrLock(pmcData, info, sessionID, output);
        // TODO: InventoryController is not implemented rn
        // return output;
        throw new NotImplementedException();
    }
}
