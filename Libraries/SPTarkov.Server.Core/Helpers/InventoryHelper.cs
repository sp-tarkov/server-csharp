using System.Collections.Frozen;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Inventory;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class InventoryHelper(
    ISptLogger<InventoryHelper> logger,
    HttpResponseUtil httpResponseUtil,
    DialogueHelper dialogueHelper,
    EventOutputHolder eventOutputHolder,
    ProfileHelper profileHelper,
    ItemHelper itemHelper,
    ServerLocalisationService serverLocalisationService,
    ConfigServer configServer,
    ICloner cloner
)
{
    private static readonly FrozenSet<MongoId> _variableSizeItemTypes =
    [
        BaseClasses.WEAPON,
        BaseClasses.FUNCTIONAL_MOD,
        BaseClasses.MOD,
    ];

    protected readonly InventoryConfig _inventoryConfig = configServer.GetConfig<InventoryConfig>();

    /// <summary>
    ///     Add multiple items to player stash (assuming they all fit)
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="request">AddItemsDirectRequest request</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="output">Client response object</param>
    public void AddItemsToStash(
        MongoId sessionId,
        AddItemsDirectRequest request,
        PmcData pmcData,
        ItemEventRouterResponse output
    )
    {
        // Check all items fit into inventory before adding
        if (!CanPlaceItemsInInventory(sessionId, request.ItemsWithModsToAdd))
        {
            // No space, exit
            httpResponseUtil.AppendErrorToOutput(
                output,
                serverLocalisationService.GetText("inventory-no_stash_space"),
                BackendErrorCodes.NotEnoughSpace
            );

            return;
        }

        foreach (var itemToAdd in request.ItemsWithModsToAdd)
        {
            var addItemRequest = new AddItemDirectRequest
            {
                ItemWithModsToAdd = itemToAdd,
                FoundInRaid = request.FoundInRaid,
                UseSortingTable = request.UseSortingTable,
                Callback = request.Callback,
            };

            // Add to player inventory
            AddItemToStash(sessionId, addItemRequest, pmcData, output);
            if (output.Warnings?.Count > 0)
            {
                return;
            }
        }
    }

    /// <summary>
    ///     Add whatever is passed in request.itemWithModsToAdd into player inventory (if it fits)
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="request">AddItemDirect request</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="output">Client response object</param>
    public void AddItemToStash(
        MongoId sessionId,
        AddItemDirectRequest request,
        PmcData pmcData,
        ItemEventRouterResponse output
    )
    {
        var itemWithModsToAddClone = cloner.Clone(request.ItemWithModsToAdd);

        // Get stash layouts ready for use
        var stashFS2D = GetStashSlotMap(pmcData);
        if (stashFS2D is null)
        {
            logger.Error($"Unable to get stash map for players: {sessionId} stash");

            return;
        }

        var sortingTableFS2D = GetSortingTableSlotMap(pmcData);

        // Find empty slot in stash for item being added - adds 'location' + parentId + slotId properties to root item
        PlaceItemInInventory(
            stashFS2D,
            sortingTableFS2D,
            itemWithModsToAddClone,
            pmcData.Inventory,
            request.UseSortingTable.GetValueOrDefault(false),
            output
        );
        if (output.Warnings?.Count > 0)
        // Failed to place, error out
        {
            return;
        }

        // Apply/remove FiR to item + mods
        SetFindInRaidStatusForItem(
            itemWithModsToAddClone,
            request.FoundInRaid.GetValueOrDefault(false)
        );

        // Remove trader properties from root item
        RemoveTraderRagfairRelatedUpdProperties(itemWithModsToAddClone[0].Upd);

        // Run callback
        try
        {
            request.Callback?.Invoke((int)(itemWithModsToAddClone[0].Upd.StackObjectsCount ?? 0));
        }
        catch (Exception ex)
        {
            // Callback failed
            var message = ex.Message;
            httpResponseUtil.AppendErrorToOutput(output, message);
            logger.Error($"[InventoryHelper]: {ex.Message}");

            return;
        }

        // Add item + mods to output and profile inventory

        output.ProfileChanges[sessionId].Items.NewItems.AddRange(itemWithModsToAddClone);
        pmcData.Inventory.Items.AddRange(itemWithModsToAddClone);

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug(
                $"Added: {itemWithModsToAddClone[0].Upd?.StackObjectsCount ?? 1} item: {itemWithModsToAddClone[0].Template} with: {itemWithModsToAddClone.Count - 1} mods to inventory"
            );
        }
    }

    /// <summary>
    ///     Set FiR status for an item + its children
    /// </summary>
    /// <param name="itemWithChildren">An item</param>
    /// <param name="foundInRaid">Item was found in raid</param>
    protected void SetFindInRaidStatusForItem(List<Item> itemWithChildren, bool foundInRaid)
    {
        foreach (var item in itemWithChildren)
        {
            // Ensure item has upd object
            itemHelper.AddUpdObjectToItem(item);

            // Ammo / currency can NEVER be FiR or have a 'SpawnedInSession' property
            item.Upd.SpawnedInSession = itemHelper.IsOfBaseclass(item.Template, BaseClasses.AMMO)
                ? null
                : foundInRaid;
        }
    }

    /// <summary>
    ///     Remove properties from a Upd object used by a trader/ragfair that are unnecessary to a player
    /// </summary>
    /// <param name="upd">Object to update</param>
    protected void RemoveTraderRagfairRelatedUpdProperties(Upd upd)
    {
        if (upd.UnlimitedCount is not null)
        {
            upd.UnlimitedCount = null;
        }

        if (upd.BuyRestrictionCurrent is not null)
        {
            upd.BuyRestrictionCurrent = null;
        }

        if (upd.BuyRestrictionMax is not null)
        {
            upd.BuyRestrictionMax = null;
        }
    }

    /// <summary>
    ///     Can all provided items be added into player inventory
    /// </summary>
    /// <param name="sessionId">Player id</param>
    /// <param name="itemsWithChildren">Array of items with children to try and fit</param>
    /// <returns>True all items fit</returns>
    public bool CanPlaceItemsInInventory(MongoId sessionId, List<List<Item>> itemsWithChildren)
    {
        var pmcData = profileHelper.GetPmcProfile(sessionId);

        var stashFS2D = cloner.Clone(GetStashSlotMap(pmcData));
        if (stashFS2D is null)
        {
            logger.Error($"Unable to get stash map for players: {sessionId} stash");

            return false;
        }

        // False if ALL items don't fit
        return itemsWithChildren.All(itemWithChildren =>
            CanPlaceItemInContainer(stashFS2D, itemWithChildren)
        );
    }

    /// <summary>
    ///     Do the provided items all fit into the grid
    /// </summary>
    /// <param name="containerFS2D">Container grid to fit items into</param>
    /// <param name="itemsWithChildren">Items to try and fit into grid</param>
    /// <returns>True all fit</returns>
    public bool CanPlaceItemsInContainer(int[,] containerFS2D, List<List<Item>> itemsWithChildren)
    {
        return itemsWithChildren.All(itemWithChildren =>
            CanPlaceItemInContainer(containerFS2D, itemWithChildren)
        );
    }

    /// <summary>
    ///     Does an item fit into a container grid
    /// </summary>
    /// <param name="containerFS2D">Container grid</param>
    /// <param name="itemWithChildren">Item to check fits</param>
    /// <returns>True it fits</returns>
    public bool CanPlaceItemInContainer(int[,] containerFS2D, List<Item> itemWithChildren)
    {
        // Get x/y size of item
        var rootItem = itemWithChildren[0];
        var (sizeX, sizeY) = GetItemSize(rootItem.Template, rootItem.Id, itemWithChildren);

        // Look for a place to slot item into
        var findSlotResult = containerFS2D.FindSlotForItem(sizeX, sizeY);
        if (findSlotResult.Success.GetValueOrDefault(false))
        {
            try
            {
                containerFS2D.FillContainerMapWithItem(
                    findSlotResult.X.Value,
                    findSlotResult.Y.Value,
                    sizeX,
                    sizeY,
                    findSlotResult.Rotation.Value
                );
            }
            catch (Exception ex)
            {
                logger.Error(
                    serverLocalisationService.GetText(
                        "inventory-unable_to_fit_item_into_inventory",
                        ex.Message
                    )
                );

                return false;
            }

            // Success! exit
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Find a free location inside a container to fit the item
    /// </summary>
    /// <param name="containerFS2D">Container grid to add item to</param>
    /// <param name="itemWithChildren">Item to add to grid</param>
    /// <param name="containerId">Id of the container we're fitting item into</param>
    /// <param name="desiredSlotId">Slot id value to use, default is "hideout"</param>
    public void PlaceItemInContainer(
        int[,] containerFS2D,
        List<Item> itemWithChildren,
        string containerId,
        string desiredSlotId = "hideout"
    )
    {
        // Get x/y size of item
        var rootItemAdded = itemWithChildren[0];
        var (sizeX, sizeY) = GetItemSize(
            rootItemAdded.Template,
            rootItemAdded.Id,
            itemWithChildren
        );

        // Look for a place to slot item into
        var findSlotResult = containerFS2D.FindSlotForItem(sizeX, sizeY);
        if (findSlotResult.Success.GetValueOrDefault(false))
        {
            try
            {
                containerFS2D.FillContainerMapWithItem(
                    findSlotResult.X.Value,
                    findSlotResult.Y.Value,
                    sizeX,
                    sizeY,
                    findSlotResult.Rotation.Value
                );
            }
            catch (Exception ex)
            {
                logger.Error(
                    serverLocalisationService.GetText("inventory-fill_container_failed", ex.Message)
                );

                return;
            }

            // Store details for object, including container item will be placed in
            rootItemAdded.ParentId = containerId;
            rootItemAdded.SlotId = desiredSlotId;
            rootItemAdded.Location = new ItemLocation
            {
                X = findSlotResult.X,
                Y = findSlotResult.Y,
                R = findSlotResult.Rotation.GetValueOrDefault(false)
                    ? ItemRotation.Vertical
                    : ItemRotation.Horizontal,
                Rotation = findSlotResult.Rotation,
            };

            // Success! exit
        }
    }

    /// <summary>
    ///     Find a location to place an item into inventory and place it
    /// </summary>
    /// <param name="stashFS2D">2-dimensional representation of the container</param>
    /// <param name="sortingTableFS2D">2-dimensional representation of the sorting table slots</param>
    /// <param name="itemWithChildren">Item to place with children</param>
    /// <param name="playerInventory">Players inventory</param>
    /// <param name="useSortingTable">Should sorting table to be used if main stash has no space</param>
    /// <param name="output">Output to send back to client</param>
    protected void PlaceItemInInventory(
        int[,] stashFS2D,
        int[,] sortingTableFS2D,
        List<Item> itemWithChildren,
        BotBaseInventory playerInventory,
        bool useSortingTable,
        ItemEventRouterResponse output
    )
    {
        // Get x/y size of item
        var rootItem = itemWithChildren[0];
        var (sizeX, sizeY) = GetItemSize(rootItem.Template, rootItem.Id, itemWithChildren);

        // Look for a place to slot item into
        var findSlotResult = stashFS2D.FindSlotForItem(sizeX, sizeY);
        if (findSlotResult.Success.Value)
        {
            try
            {
                stashFS2D.FillContainerMapWithItem(
                    findSlotResult.X.Value,
                    findSlotResult.Y.Value,
                    sizeX,
                    sizeY,
                    findSlotResult.Rotation.Value
                );
            }
            catch (Exception ex)
            {
                HandleContainerPlacementError(ex.Message, output);

                return;
            }

            // Store details for object, including container item will be placed in
            rootItem.ParentId = playerInventory.Stash;
            rootItem.SlotId = "hideout";
            rootItem.Location = new ItemLocation
            {
                X = findSlotResult.X,
                Y = findSlotResult.Y,
                R = findSlotResult.Rotation.Value ? ItemRotation.Vertical : ItemRotation.Horizontal,
                Rotation = findSlotResult.Rotation,
            };

            // Success! exit
            return;
        }

        // Space not found in main stash, use sorting table
        if (useSortingTable)
        {
            var findSortingSlotResult = sortingTableFS2D.FindSlotForItem(sizeX, sizeY);

            try
            {
                sortingTableFS2D.FillContainerMapWithItem(
                    findSortingSlotResult.X.Value,
                    findSortingSlotResult.Y.Value,
                    sizeX,
                    sizeY,
                    findSortingSlotResult.Rotation.Value
                );
            }
            catch (Exception ex)
            {
                HandleContainerPlacementError(ex.Message, output);

                return;
            }

            // Store details for object, incuding container item will be placed in
            itemWithChildren[0].ParentId = playerInventory.SortingTable;
            itemWithChildren[0].Location = new ItemLocation
            {
                X = findSortingSlotResult.X,
                Y = findSortingSlotResult.Y,
                R = findSortingSlotResult.Rotation.Value
                    ? ItemRotation.Vertical
                    : ItemRotation.Horizontal,
                Rotation = findSortingSlotResult.Rotation,
            };
        }
        else
        {
            httpResponseUtil.AppendErrorToOutput(
                output,
                serverLocalisationService.GetText("inventory-no_stash_space"),
                BackendErrorCodes.NotEnoughSpace
            );
        }
    }

    protected void HandleContainerPlacementError(string errorText, ItemEventRouterResponse output)
    {
        logger.Error(
            serverLocalisationService.GetText("inventory-fill_container_failed", errorText)
        );

        httpResponseUtil.AppendErrorToOutput(
            output,
            serverLocalisationService.GetText("inventory-no_stash_space")
        );
    }

    /// <summary>
    ///     Handle Remove event
    ///     Remove item from player inventory + insured items array
    ///     Also deletes child items
    /// </summary>
    /// <param name="profile">Profile to remove item from (pmc or scav)</param>
    /// <param name="itemId">Items id to remove</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="output">OPTIONAL - ItemEventRouterResponse</param>
    public void RemoveItem(
        PmcData profile,
        MongoId itemId,
        MongoId sessionId,
        ItemEventRouterResponse? output = null
    )
    {
        if (itemId.IsEmpty())
        {
            logger.Warning(
                serverLocalisationService.GetText("inventory-unable_to_remove_item_no_id_given")
            );

            return;
        }

        // Get children of item, they get deleted too
        var itemAndChildrenToRemove = profile.Inventory.Items.FindAndReturnChildrenAsItems(itemId);
        if (!itemAndChildrenToRemove.Any())
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug(
                    serverLocalisationService.GetText(
                        "inventory-unable_to_remove_item_id_not_found",
                        new { ChildId = itemId, ProfileId = profile.Id }
                    )
                );
            }

            return;
        }

        var inventoryItems = profile.Inventory.Items;
        var insuredItems = profile.InsuredItems;

        // We have output object, inform client of root item deletion, not children
        output?.ProfileChanges[sessionId].Items.DeletedItems.Add(new DeletedItem { Id = itemId });

        foreach (var item in itemAndChildrenToRemove)
        {
            // We expect that each inventory item and each insured item has unique "_id", respective "itemId".
            // Therefore, we want to use a NON-Greedy function and escape the iteration as soon as we find requested item.
            var inventoryIndex = inventoryItems.FindIndex(inventoryItem =>
                inventoryItem.Id == item.Id
            );
            if (inventoryIndex != -1)
            {
                inventoryItems.RemoveAt(inventoryIndex);
            }
            else
            {
                logger.Warning(
                    serverLocalisationService.GetText(
                        "inventory-unable_to_remove_item_id_not_found",
                        new { childId = item.Id, ProfileId = profile.Id }
                    )
                );
            }

            var insuredItemIndex = insuredItems.FindIndex(insuredItem =>
                insuredItem.ItemId == item.Id
            );
            if (insuredItemIndex != -1)
            {
                insuredItems.RemoveAt(insuredItemIndex);
            }
        }
    }

    /// <summary>
    ///     Delete desired item from a player profiles mail
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="removeRequest">Remove request</param>
    /// <param name="output">OPTIONAL - ItemEventRouterResponse</param>
    public void RemoveItemAndChildrenFromMailRewards(
        MongoId sessionId,
        InventoryRemoveRequestData removeRequest,
        ItemEventRouterResponse? output
    )
    {
        var fullProfile = profileHelper.GetFullProfile(sessionId);

        // Iterate over all dialogs and look for mesasage with key from request, that has item (and maybe its children) we want to remove
        var dialogs = fullProfile.DialogueRecords;
        foreach (var (_, dialog) in dialogs)
        {
            var messageWithReward = dialog.Messages.FirstOrDefault(message =>
                message.Id == removeRequest.FromOwner.Id
            );
            if (messageWithReward is not null)
            {
                // Find item + any possible children and remove them from mails items array
                var itemWithChildren = messageWithReward.Items.Data.FindAndReturnChildrenAsItems(
                    removeRequest.Item
                );
                foreach (var itemToDelete in itemWithChildren)
                {
                    // Get index of item to remove from reward array + remove it
                    var indexOfItemToRemove = messageWithReward.Items.Data.IndexOf(itemToDelete);
                    if (indexOfItemToRemove == -1)
                    {
                        logger.Error(
                            serverLocalisationService.GetText(
                                "inventory-unable_to_remove_item_restart_immediately",
                                new
                                {
                                    item = removeRequest.Item,
                                    mailId = removeRequest.FromOwner.Id,
                                }
                            )
                        );

                        continue;
                    }

                    messageWithReward.Items.Data.RemoveAt(indexOfItemToRemove);
                }

                // Flag message as having no rewards if all removed
                var hasRewardItemsRemaining = messageWithReward?.Items.Data?.Count > 0;
                messageWithReward.HasRewards = hasRewardItemsRemaining;
                messageWithReward.RewardCollected = !hasRewardItemsRemaining;
            }
        }
    }

    /// <summary>
    ///     Find item by id in player inventory and remove x of its count
    /// </summary>
    /// <param name="pmcData">player profile</param>
    /// <param name="itemId">Item id to decrement StackObjectsCount of</param>
    /// <param name="countToRemove">Number of item to remove</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="output">ItemEventRouterResponse</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse RemoveItemByCount(
        PmcData pmcData,
        MongoId itemId,
        int countToRemove,
        MongoId sessionId,
        ItemEventRouterResponse? output
    )
    {
        if (itemId.IsEmpty())
        {
            return output;
        }

        // Goal is to keep removing items until we can remove part of an items stack
        var itemsToReduce = pmcData.Inventory.Items.FindAndReturnChildrenAsItems(itemId);
        var remainingCount = countToRemove;
        foreach (var itemToReduce in itemsToReduce)
        {
            var itemStackSize = itemToReduce.GetItemStackSize();

            // Remove whole stack
            if (remainingCount >= itemStackSize)
            {
                remainingCount -= itemStackSize;
                RemoveItem(pmcData, itemToReduce.Id, sessionId, output);
            }
            else
            {
                itemToReduce.Upd.StackObjectsCount -= remainingCount;
                remainingCount = 0;
                if (output is not null)
                {
                    output.ProfileChanges[sessionId].Items.ChangedItems.Add(itemToReduce);
                }
            }

            if (remainingCount == 0)
            // Desired count of item has been removed / we ran out of items to remove
            {
                break;
            }
        }

        return output ?? eventOutputHolder.GetOutput(sessionId);
    }

    /// <summary>
    ///     Get the height and width of an item - can have children that alter size
    /// </summary>
    /// <param name="itemTpl">Item to get size of</param>
    /// <param name="itemId">Items id to get size of</param>
    /// <param name="inventoryItems"></param>
    /// <returns>[width, height]</returns>
    public (int, int) GetItemSize(MongoId itemTpl, MongoId itemId, List<Item> inventoryItems)
    {
        // -> Prepares item Width and height returns [sizeX, sizeY]
        return GetSizeByInventoryItemHash(itemTpl, itemId, inventoryItems.GetInventoryItemHash());
    }

    /// <summary>
    ///     Calculates the size of an item including attachments
    ///     takes into account if item is folded
    /// </summary>
    /// <param name="itemTpl">Items template id</param>
    /// <param name="itemId">Items id</param>
    /// <param name="inventoryItemHash">Hashmap of inventory items</param>
    /// <returns>An array representing the [width, height] of the item</returns>
    protected (int, int) GetSizeByInventoryItemHash(
        MongoId itemTpl,
        MongoId itemId,
        InventoryItemHash inventoryItemHash
    )
    {
        // Invalid item
        var (isValidItem, itemTemplate) = itemHelper.GetItem(itemTpl);
        if (!isValidItem)
        {
            logger.Error(
                serverLocalisationService.GetText("inventory-invalid_item_missing_from_db", itemTpl)
            );
        }

        // Item found but no _props property
        if (isValidItem && itemTemplate.Properties is null)
        {
            serverLocalisationService.GetText(
                "inventory-item_missing_props_property",
                new { itemTpl, itemName = itemTemplate?.Name }
            );
        }

        // No item object or getItem() returned false
        if (!isValidItem && itemTemplate is null)
        {
            // return default size of 1x1
            logger.Error(
                serverLocalisationService.GetText("inventory-return_default_size", itemTpl)
            );

            return (1, 1); // Invalid input data, return defaults
        }

        if (!inventoryItemHash.ByItemId.TryGetValue(itemId, out var rootItem))
        {
            logger.Error(
                $"Unable to get root item with Id: {itemId} from player inventory. Defaulting to 1x1"
            );

            return (1, 1); // Invalid input data, return defaults
        }

        // Does root item support being folded
        var rootCanBeFolded = itemTemplate.Properties.Foldable.GetValueOrDefault(false);

        // The slot that can be folded on root e.g. "mod_stock"
        var foldedSlot = itemTemplate.Properties.FoldedSlot;

        int sizeUp = 0,
            sizeDown = 0,
            sizeLeft = 0,
            sizeRight = 0;
        int forcedUp = 0,
            forcedDown = 0,
            forcedLeft = 0,
            forcedRight = 0;
        var outX = itemTemplate.Properties.Width;
        var outY = itemTemplate.Properties.Height;

        // Is the root item actively folded
        var rootIsFolded = rootItem?.Upd?.Foldable?.Folded.GetValueOrDefault(false) ?? false;

        // Root can be collapsed and has been collapsed
        if (rootCanBeFolded && string.IsNullOrEmpty(foldedSlot) && rootIsFolded)
        {
            // foldedSlot must be empty/null which means the root item itself is folded, not a sub child item...i think
            outX -= itemTemplate.Properties.SizeReduceRight.Value;
        }

        // Item can have child items that adjust its size
        if (itemHelper.IsOfBaseclasses(itemTpl, _variableSizeItemTypes))
        {
            // Storage for root item and its children, store root item id for now
            // Will store child items that may have sub-children to process
            var toDo = new Queue<MongoId>([itemId]);
            while (toDo.Count > 0)
            {
                // Lookup parent in `to do queue`, get all of its children, then loop over them
                if (inventoryItemHash.ByParentId.TryGetValue(toDo.Peek(), out var children))
                {
                    foreach (var childItem in children)
                    {
                        // Skip mods that don't increase size. e.g. cartridges
                        if (
                            !childItem.SlotId.StartsWith("mod_", StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            continue;
                        }

                        // Add child to processing queue to be checked for sub-children later
                        toDo.Enqueue(childItem.Id);

                        // Get child item from db
                        var (isValid, template) = itemHelper.GetItem(childItem.Template);
                        if (!isValid)
                        {
                            logger.Error(
                                serverLocalisationService.GetText(
                                    "inventory-get_item_size_item_not_found_by_tpl",
                                    childItem.Template
                                )
                            );
                        }

                        var childCanBeFolded = template.Properties.Foldable.GetValueOrDefault(
                            false
                        );
                        var childIsFolded =
                            childItem.Upd?.Foldable?.Folded.GetValueOrDefault(false) ?? false;

                        if (
                            rootCanBeFolded
                            && foldedSlot == childItem.SlotId
                            && (rootIsFolded || childIsFolded)
                        )
                        {
                            continue;
                        }

                        // Child mod can and is folded, don't include it in size calc
                        if (childCanBeFolded && rootIsFolded && childIsFolded)
                        {
                            continue;
                        }

                        // Calculating child ExtraSize
                        if (template.Properties.ExtraSizeForceAdd.GetValueOrDefault(false))
                        {
                            forcedUp += template.Properties.ExtraSizeUp.Value;
                            forcedDown += template.Properties.ExtraSizeDown.Value;
                            forcedLeft += template.Properties.ExtraSizeLeft.Value;
                            forcedRight += template.Properties.ExtraSizeRight.Value;
                        }
                        else
                        {
                            sizeUp =
                                sizeUp < template.Properties.ExtraSizeUp
                                    ? template.Properties.ExtraSizeUp.Value
                                    : sizeUp;
                            sizeDown =
                                sizeDown < template.Properties.ExtraSizeDown
                                    ? template.Properties.ExtraSizeDown.Value
                                    : sizeDown;
                            sizeLeft =
                                sizeLeft < template.Properties.ExtraSizeLeft
                                    ? template.Properties.ExtraSizeLeft.Value
                                    : sizeLeft;
                            sizeRight =
                                sizeRight < template.Properties.ExtraSizeRight
                                    ? template.Properties.ExtraSizeRight.Value
                                    : sizeRight;
                        }
                    }
                }

                // Item has been processed, remove from queue
                toDo.Dequeue();
            }
        }

        return (
            outX.Value + sizeLeft + sizeRight + forcedLeft + forcedRight,
            outY.Value + sizeUp + sizeDown + forcedUp + forcedDown
        );
    }

    /// <summary>
    ///     Get a 2d mapping of a container with what grid slots are filled
    /// </summary>
    /// <param name="containerSizeHorizontalX">Horizontal (Column) size of container</param>
    /// <param name="containerSizeVerticalY">Vertical (Row) size of container</param>
    /// <param name="itemList">Players inventory items</param>
    /// <param name="containerId">Id of the container</param>
    /// <returns>Two-dimensional representation of container</returns>
    public int[,] GetContainerMap(
        int containerSizeHorizontalX,
        int containerSizeVerticalY,
        List<Item> itemList,
        MongoId containerId
    )
    {
        // Create blank 2d map of container
        var container = itemHelper.GetBlankContainerMap(
            containerSizeHorizontalX, // Column count
            containerSizeVerticalY // Row count
        );

        // Get all items in players inventory keyed by their parentId and by ItemId
        var inventoryItemHash = itemList.GetInventoryItemHash();

        // Get subset of items that belong to the desired container
        if (!inventoryItemHash.ByParentId.TryGetValue(containerId, out var rootItemsInContainer))
        {
            // No items in container, exit early and return the blank container map
            return container;
        }

        // Add every root items size (with mods attached) found in container
        foreach (var rootItem in rootItemsInContainer)
        {
            var itemLocation = rootItem.GetParsedLocation();
            if (itemLocation is null)
            {
                // Item has no location property
                logger.Error(
                    $"Unable to find 'location' property on item with id: {rootItem.Id}, skipping"
                );

                continue;
            }

            // Get x/y size of item (without rotation)
            var (rawItemXWidth, rawItemYHeight) = GetSizeByInventoryItemHash(
                rootItem.Template,
                rootItem.Id,
                inventoryItemHash
            );
            // Items horizontal size
            var itemHeight = itemLocation.IsVertical() ? rawItemXWidth : rawItemYHeight;

            // Items vertical size
            var itemWidth = itemLocation.IsVertical() ? rawItemYHeight : rawItemXWidth;

            // vertical (row)
            for (var yOffset = 0; yOffset < itemHeight; yOffset++)
            {
                // horizontal (column)
                for (var xOffset = 0; xOffset < itemWidth; xOffset++)
                {
                    var currentY = itemLocation.Y.Value + yOffset;
                    var currentX = itemLocation.X.Value + xOffset;

                    // Check still in containers bounds
                    if (
                        currentY >= 0
                        && currentY < containerSizeVerticalY
                        && currentX >= 0
                        && currentX < containerSizeHorizontalX
                    )
                    {
                        // mark slot used
                        container[currentY, currentX] = 1;
                    }
                    else
                    {
                        // Out of bounds
                        var message =
                            $"Item: {rootItem.Id} at: {itemLocation.X}, {itemLocation.Y} size: {itemHeight}x{itemWidth} extends outside the containers bounds";

                        logger.Error(
                            serverLocalisationService.GetText(
                                "inventory-unable_to_fill_container",
                                new { id = rootItem.Id, error = $"{message}" }
                            )
                        );

                        // Stop and try next row
                        break;
                    }
                }
            }
        }

        return container;
    }

    /// <summary>
    ///     Return the inventory that needs to be modified (scav/pmc etc)
    ///     Changes made to result apply to character inventory
    ///     Based on the item action, determine whose inventories we should be looking at for from and to.
    /// </summary>
    /// <param name="request">Item interaction request</param>
    /// <param name="itemId">Item being moved/split/etc to inventory</param>
    /// <param name="sessionId">Session id / players Id</param>
    /// <returns>OwnerInventoryItems with inventory of player/scav to adjust</returns>
    public OwnerInventoryItems GetOwnerInventoryItems(
        InventoryBaseActionRequestData request,
        MongoId itemId,
        MongoId sessionId
    )
    {
        var pmcItems = profileHelper.GetPmcProfile(sessionId).Inventory.Items;
        var scavProfile = profileHelper.GetScavProfile(sessionId);
        var fromInventoryItems = pmcItems;
        var fromType = "pmc";

        if (request.FromOwner is not null)
        {
            if (request.FromOwner.Id == scavProfile.Id)
            {
                fromInventoryItems = scavProfile.Inventory.Items;
                fromType = "scav";
            }
            else if (
                string.Equals(request.FromOwner.Type, "mail", StringComparison.OrdinalIgnoreCase)
            )
            {
                // Split requests don't use 'use' but 'splitItem' property
                fromInventoryItems = dialogueHelper.GetMessageItemContents(
                    request.FromOwner.Id.Value,
                    sessionId,
                    itemId
                );
                fromType = "mail";
            }
        }

        // Don't need to worry about mail for destination because client doesn't allow
        // users to move items back into the mail stash.
        var toInventoryItems = pmcItems;
        var toType = "pmc";

        // Destination is scav inventory, update values
        if (request.ToOwner?.Id == scavProfile.Id)
        {
            toInventoryItems = scavProfile.Inventory.Items;
            toType = "scav";
        }

        // From and To types match, same inventory
        var movingToSameInventory = fromType == toType;

        return new OwnerInventoryItems
        {
            From = fromInventoryItems,
            To = toInventoryItems,
            SameInventory = movingToSameInventory,
            IsMail = fromType == "mail",
        };
    }

    /// <summary>
    ///     Get a two-dimensional array to represent stash slots
    ///     0 value = free, 1 = taken
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <returns>2-dimensional array</returns>
    protected int[,] GetStashSlotMap(PmcData pmcData)
    {
        var (horizontal, vertical) = GetPlayerStashSize(pmcData);
        return GetContainerMap(
            horizontal,
            vertical,
            pmcData.Inventory.Items,
            pmcData.Inventory.Stash.Value
        );
    }

    /// <summary>
    ///     Get a blank two-dimensional array representation of a container
    /// </summary>
    /// <param name="containerTpl">Container to get data for</param>
    /// <returns>blank two-dimensional array</returns>
    public int[,] GetContainerSlotMap(string containerTpl)
    {
        var containerTemplate = itemHelper.GetItem(containerTpl).Value;

        var firstContainerGrid = containerTemplate.Properties.Grids.FirstOrDefault();
        var containerRowCount = firstContainerGrid.Props.CellsH;
        var containerColumnCount = firstContainerGrid.Props.CellsV;

        return itemHelper.GetBlankContainerMap(containerColumnCount.Value, containerRowCount.Value);
    }

    /// <summary>
    ///     Get a two-dimensional array representation of the players sorting table
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <returns>two-dimensional array</returns>
    protected int[,] GetSortingTableSlotMap(PmcData pmcData)
    {
        return GetContainerMap(
            10,
            45,
            pmcData.Inventory.Items,
            pmcData.Inventory.SortingTable.Value
        );
    }

    /// <summary>
    ///     Get Players Stash Size
    /// </summary>
    /// <param name="pmcData">Profile to get stash size of</param>
    /// <returns>Horizontal and vertical size of stash</returns>
    protected (int, int) GetPlayerStashSize(PmcData pmcData)
    {
        // TODO: what??
        // This sets automatically a stash size from items.json (it's not added anywhere yet because we still use base stash)
        var stashTpl = GetProfileStashTpl(pmcData);
        if (stashTpl is null)
        {
            logger.Error(serverLocalisationService.GetText("inventory-missing_stash_size"));

            return (0, 0);
        }

        // Look up details of stash in db
        var (isValidItem, stashItemDbItem) = itemHelper.GetItem(stashTpl);
        if (!isValidItem)
        {
            logger.Error(serverLocalisationService.GetText("inventory-stash_not_found", stashTpl));

            return (0, 0);
        }

        // Find the main 'grid' of the stash we can use to get size
        var firstStashItemGrid = stashItemDbItem?.Properties?.Grids?.FirstOrDefault();

        // Get horizontal and vertical size
        var stashH = firstStashItemGrid.Props.CellsH != 0 ? firstStashItemGrid.Props.CellsH : 10;
        var stashV = firstStashItemGrid.Props.CellsV != 0 ? firstStashItemGrid.Props.CellsV : 66;

        // Player has a bonus, apply to vertical size
        var stashRowBonus = pmcData.Bonuses.FirstOrDefault(bonus =>
            bonus.Type == BonusType.StashRows
        );
        if (stashRowBonus is not null)
        {
            stashV += (int)stashRowBonus.Value;
        }

        return (stashH.Value, stashV.Value);
    }

    /// <summary>
    ///     Get the players stash items tpl
    /// </summary>
    /// <param name="profile">Profile to get tpl</param>
    /// <returns>Stash tpl</returns>
    protected string? GetProfileStashTpl(PmcData profile)
    {
        var stashObj = profile.Inventory.Items.FirstOrDefault(item =>
            item.Id == profile.Inventory.Stash
        );
        if (stashObj is null)
        {
            logger.Error(serverLocalisationService.GetText("inventory-unable_to_find_stash"));
        }

        return stashObj?.Template;
    }

    /// <summary>
    ///     Internal helper function to transfer an item + children from one profile to another.
    /// </summary>
    /// <param name="sourceItems">Inventory of the source (can be non-player)</param>
    /// <param name="toItems">Inventory of the destination</param>
    /// <param name="request">Move request</param>
    public void MoveItemToProfile(
        List<Item> sourceItems,
        List<Item> toItems,
        InventoryMoveRequestData request
    )
    {
        HandleCartridgeMove(sourceItems, request);

        // Get all children item has, they need to move with item
        var idsToMove = sourceItems.FindAndReturnChildrenByItems(request.Item.Value);
        foreach (var itemId in idsToMove)
        {
            var itemToMove = sourceItems.FirstOrDefault(item => item.Id == itemId);
            if (itemToMove is null)
            {
                logger.Error(
                    serverLocalisationService.GetText(
                        "inventory-unable_to_find_item_to_move",
                        itemId
                    )
                );
                continue;
            }

            // Only adjust the values for parent item, not children (their values are already correctly tied to parent)
            if (itemId == request.Item)
            {
                itemToMove.ParentId = request.To.Id;
                itemToMove.SlotId = request.To.Container;

                // Update location object
                itemToMove.Location = request.To.Location ?? null; // No location in request, delete it
            }

            toItems.Add(itemToMove);
            sourceItems.RemoveAt(sourceItems.IndexOf(itemToMove));
        }
    }

    /// <summary>
    ///     Internal helper function to move item within the same profile.
    /// </summary>
    /// <param name="pmcData">profile to edit</param>
    /// <param name="inventoryItems"></param>
    /// <param name="moveRequest">client move request</param>
    /// <param name="errorMessage"></param>
    /// <returns>True if move was successful</returns>
    public bool MoveItemInternal(
        PmcData pmcData,
        List<Item> inventoryItems,
        InventoryMoveRequestData moveRequest,
        out string errorMessage
    )
    {
        errorMessage = string.Empty;
        HandleCartridgeMove(inventoryItems, moveRequest);

        // Find item we want to 'move'
        var matchingInventoryItem = inventoryItems.FirstOrDefault(item =>
            item.Id == moveRequest.Item
        );
        if (matchingInventoryItem is null)
        {
            var noMatchingItemMesage =
                $"Unable to move item: {moveRequest.Item}, cannot find in inventory";
            logger.Error(noMatchingItemMesage);

            errorMessage = noMatchingItemMesage;
            return false;
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug(
                $"{moveRequest.Action} item: {moveRequest.Item} from slotid: {matchingInventoryItem.SlotId} to container: {moveRequest.To.Container}"
            );
        }

        // Don't move shells from camora to cartridges (happens when loading shells into mts-255 revolver shotgun)
        if (
            matchingInventoryItem.SlotId?.Contains("camora_") is null
            && moveRequest.To.Container == "cartridges"
        )
        {
            logger.Warning(
                serverLocalisationService.GetText(
                    "inventory-invalid_move_to_container",
                    new
                    {
                        slotId = matchingInventoryItem.SlotId,
                        container = moveRequest.To.Container,
                    }
                )
            );

            return true;
        }

        // Edit items details to match its new location
        matchingInventoryItem.ParentId = moveRequest.To.Id;
        matchingInventoryItem.SlotId = moveRequest.To.Container;

        // Ensure fastpanel dict updates when item was moved out of fast-panel-accessible slot
        UpdateFastPanelBinding(pmcData, matchingInventoryItem);

        // Item has location property, ensure its value is handled
        if (moveRequest.To.Location is not null)
        {
            matchingInventoryItem.Location = moveRequest.To.Location;
        }
        else
        {
            // Moved from slot with location to one without, clean up
            if (matchingInventoryItem.Location is not null)
            {
                matchingInventoryItem.Location = null;
            }
        }

        return true;
    }

    /// <summary>
    ///     Update fast panel bindings when an item is moved into a container that doesn't allow quick slot access
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="itemBeingMoved">item being moved</param>
    protected void UpdateFastPanelBinding(PmcData pmcData, Item itemBeingMoved)
    {
        // Find matching _id in fast panel

        if (!pmcData.Inventory.FastPanel.TryGetValue(itemBeingMoved.Id, out var fastPanelSlot))
        {
            return;
        }

        // Get moved items parent (should be container item was put into)
        var itemParent = pmcData.Inventory.Items.FirstOrDefault(item =>
            item.Id == itemBeingMoved.ParentId
        );
        if (itemParent is null)
        {
            return;
        }

        // Reset fast panel value if item was moved to a container other than pocket/rig (cant be used from fastpanel)
        HashSet<string> slots = ["pockets", "tacticalvest"];
        var wasMovedToFastPanelAccessibleContainer = slots.Contains(
            itemParent?.SlotId?.ToLowerInvariant() ?? ""
        );
        if (!wasMovedToFastPanelAccessibleContainer)
        {
            pmcData.Inventory.FastPanel[fastPanelSlot.ToString()] = "";
        }
    }

    /// <summary>
    /// Helper function to handle cartridges in inventory if any of them exist.
    /// </summary>
    /// <param name="items"></param>
    /// <param name="request"></param>
    protected void HandleCartridgeMove(List<Item> items, InventoryMoveRequestData request)
    {
        // Not moving item into a cartridge slot, skip
        if (request.To.Container != "cartridges")
        {
            return;
        }

        // Get a count of cartridges in existing magazine
        var cartridgeCount = items.Count(item => item.ParentId == request.To.Id);

        request.To.Location = cartridgeCount;
    }

    /// <summary>
    ///     Get details for how a random loot container should be handled, max rewards, possible reward tpls
    /// </summary>
    /// <param name="itemTpl">Container being opened</param>
    /// <returns>Reward details</returns>
    public RewardDetails? GetRandomLootContainerRewardDetails(MongoId itemTpl)
    {
        _inventoryConfig.RandomLootContainers.TryGetValue(itemTpl, out var result);

        return result;
    }

    /// <summary>
    ///     Get inventory configuration
    /// </summary>
    /// <returns>Inventory configuration</returns>
    public InventoryConfig GetInventoryConfig()
    {
        return _inventoryConfig;
    }

    public void ValidateInventoryUsesMongoIds(List<Item> itemsToValidate)
    {
        var errors = itemsToValidate
            .Where(item => !item.Id.IsValidMongoId())
            .Select(item => $"Id: {item.Id} - tpl: {item.Template}")
            .ToList();
        foreach (var message in errors)
        {
            logger.Error(message);
        }

        throw new Exception(
            "This profile is not compatible with SPT, See above for a list of incompatible IDs that is not compatible. Loading of SPT has been halted, use another profile or create a new one"
        );
    }
}

public class InventoryItemHash
{
    [JsonPropertyName("byItemId")]
    public Dictionary<MongoId, Item> ByItemId { get; set; }

    [JsonPropertyName("byParentId")]
    public Dictionary<MongoId, HashSet<Item>> ByParentId { get; set; }
}
