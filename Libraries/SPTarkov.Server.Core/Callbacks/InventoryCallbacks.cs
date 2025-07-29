using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Quests;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class InventoryCallbacks(InventoryController inventoryController, QuestController questController)
{
    /// <summary>
    ///     Handle client/game/profile/items/moving Move event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse MoveItem(
        PmcData pmcData,
        InventoryMoveRequestData info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.MoveItem(pmcData, info, sessionID, output);
        return output;
    }

    /// <summary>
    ///     Handle Remove event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse RemoveItem(
        PmcData pmcData,
        InventoryRemoveRequestData info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.DiscardItem(pmcData, info, sessionID, output);
        return output;
    }

    /// <summary>
    ///     Handle Split event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse SplitItem(
        PmcData pmcData,
        InventorySplitRequestData info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.SplitItem(pmcData, info, sessionID, output);
        return output;
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse MergeItem(
        PmcData pmcData,
        InventoryMergeRequestData info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.MergeItem(pmcData, info, sessionID, output);
        return output;
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse TransferItem(
        PmcData pmcData,
        InventoryTransferRequestData info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.TransferItem(pmcData, info, sessionID, output);
        return output;
    }

    /// <summary>
    ///     Handle Swap
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse SwapItem(PmcData pmcData, InventorySwapRequestData info, MongoId sessionID)
    {
        return inventoryController.SwapItem(pmcData, info, sessionID);
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse FoldItem(PmcData pmcData, InventoryFoldRequestData info, MongoId sessionID)
    {
        return inventoryController.FoldItem(pmcData, info, sessionID);
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse ToggleItem(PmcData pmcData, InventoryToggleRequestData info, MongoId sessionID)
    {
        return inventoryController.ToggleItem(pmcData, info, sessionID);
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request"></param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse TagItem(PmcData pmcData, InventoryTagRequestData request, MongoId sessionId)
    {
        return inventoryController.TagItem(pmcData, request, sessionId);
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse BindItem(
        PmcData pmcData,
        InventoryBindRequestData info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.BindItem(pmcData, info, sessionID, output);
        return output;
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse UnBindItem(
        PmcData pmcData,
        InventoryBindRequestData info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.UnBindItem(pmcData, info, sessionID, output);
        return output;
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse ExamineItem(
        PmcData pmcData,
        InventoryExamineRequestData info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.ExamineItem(pmcData, info, sessionID, output);
        return output;
    }

    /// <summary>
    ///     Handle ReadEncyclopedia
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse ReadEncyclopedia(PmcData pmcData, InventoryReadEncyclopediaRequestData info, MongoId sessionID)
    {
        return inventoryController.ReadEncyclopedia(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle ApplyInventoryChanges
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse SortInventory(
        PmcData pmcData,
        InventorySortRequestData info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.SortInventory(pmcData, info, sessionID, output);
        return output;
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse CreateMapMarker(
        PmcData pmcData,
        InventoryCreateMarkerRequestData info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.CreateMapMarker(pmcData, info, sessionID, output);
        return output;
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse DeleteMapMarker(
        PmcData pmcData,
        InventoryDeleteMarkerRequestData info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.DeleteMapMarker(pmcData, info, sessionID, output);
        return output;
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse EditMapMarker(
        PmcData pmcData,
        InventoryEditMarkerRequestData info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.EditMapMarker(pmcData, info, sessionID, output);
        return output;
    }

    /// <summary>
    ///     Handle OpenRandomLootContainer
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse OpenRandomLootContainer(
        PmcData pmcData,
        OpenRandomLootContainerRequestData info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.OpenRandomLootContainer(pmcData, info, sessionID, output);
        return output;
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse RedeemProfileReward(
        PmcData pmcData,
        RedeemProfileRequestData info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.RedeemProfileReward(pmcData, info, sessionID);
        return output;
    }

    /// <summary>
    ///     Handle game/profile/items/moving SetFavoriteItems
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse SetFavoriteItem(
        PmcData pmcData,
        SetFavoriteItems info,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        inventoryController.SetFavoriteItem(pmcData, info, sessionID);
        return output;
    }

    /// <summary>
    ///     TODO: MOVE INTO QUEST CODE
    ///     Handle game/profile/items/moving - QuestFail
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse FailQuest(PmcData pmcData, FailQuestRequestData info, MongoId sessionID, ItemEventRouterResponse output)
    {
        questController.FailQuest(pmcData, info, sessionID, output);
        return output;
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    /// <returns></returns>
    public ItemEventRouterResponse PinOrLock(PmcData pmcData, PinOrLockItemRequest info, MongoId sessionID, ItemEventRouterResponse output)
    {
        inventoryController.PinOrLock(pmcData, info, sessionID, output);
        return output;
    }
}
