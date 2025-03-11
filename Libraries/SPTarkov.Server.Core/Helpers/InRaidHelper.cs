using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Common.Annotations;
using SPTarkov.Common.Extensions;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class InRaidHelper(
    InventoryHelper _inventoryHelper,
    ItemHelper _itemHelper,
    ConfigServer _configServer,
    ICloner _cloner,
    DatabaseService _databaseService
)
{
    protected InRaidConfig _inRaidConfig = _configServer.GetConfig<InRaidConfig>();
    protected LostOnDeathConfig _lostOnDeathConfig = _configServer.GetConfig<LostOnDeathConfig>();
    protected static readonly List<string> _pocketSlots = ["pocket1", "pocket2", "pocket3", "pocket4"];

    /// <summary>
    ///     Deprecated. Reset the skill points earned in a raid to 0, ready for next raid.
    /// </summary>
    /// <param name="profile">Profile to update</param>
    protected void ResetSkillPointsEarnedDuringRaid(PmcData profile)
    {
        foreach (var skill in profile.Skills.Common)
        {
            skill.PointsEarnedDuringSession = 0.0;
        }
    }

    /// <summary>
    ///     Update a player's inventory post-raid.
    ///     Remove equipped items from pre-raid.
    ///     Add new items found in raid to profile.
    ///     Store insurance items in profile.
    /// </summary>
    /// <param name="sessionID">Session id</param>
    /// <param name="serverProfile">Profile to update</param>
    /// <param name="postRaidProfile">Profile returned by client after a raid</param>
    /// <param name="isSurvived">Indicates if the player survived the raid</param>
    /// <param name="isTransfer">Indicates if it is a transfer operation</param>
    public void SetInventory(
        string sessionID,
        PmcData serverProfile,
        PmcData postRaidProfile,
        bool isSurvived,
        bool isTransfer)
    {
        // Store insurance (as removeItem() removes insured items)
        var insured = _cloner.Clone(serverProfile.InsuredItems);

        // Remove equipment and loot items stored on player from server profile in preparation for data from client being added
        _inventoryHelper.RemoveItem(serverProfile, serverProfile.Inventory.Equipment, sessionID);

        // Remove quest items stored on player from server profile in preparation for data from client being added
        _inventoryHelper.RemoveItem(serverProfile, serverProfile.Inventory.QuestRaidItems, sessionID);

        // Get all items that have a parent of `serverProfile.Inventory.equipment` (All items player had on them at end of raid)
        var postRaidInventoryItems = _itemHelper.FindAndReturnChildrenAsItems(
            postRaidProfile.Inventory.Items,
            postRaidProfile.Inventory.Equipment
        );

        // Get all items that have a parent of `serverProfile.Inventory.questRaidItems` (Quest items player had on them at end of raid)
        var postRaidQuestItems = _itemHelper.FindAndReturnChildrenAsItems(
            postRaidProfile.Inventory.Items,
            postRaidProfile.Inventory.QuestRaidItems
        );

        // Handle Removing of FIR status if player did not survive + not transferring
        // Do after above filtering code to reduce work done
        if (!isSurvived && !isTransfer && !_inRaidConfig.AlwaysKeepFoundInRaidOnRaidEnd)
        {
            RemoveFiRStatusFromCertainItems(postRaidProfile.Inventory.Items);
        }

        // Add items from client profile into server profile
        AddItemsToInventory(postRaidInventoryItems, serverProfile.Inventory.Items);

        // Add quest items from client profile into server profile
        AddItemsToInventory(postRaidQuestItems, serverProfile.Inventory.Items);

        serverProfile.Inventory.FastPanel = postRaidProfile.Inventory.FastPanel; // Quick access items bar
        serverProfile.InsuredItems = insured;
    }

    /// <summary>
    ///     Remove FiR status from items.
    /// </summary>
    /// <param name="items">Items to process</param>
    protected void RemoveFiRStatusFromCertainItems(List<Item> items)
    {
        var dbItems = _databaseService.GetItems();

        var itemsToRemovePropertyFrom = items.Where(
            item =>
            {
                // Has upd object + upd.SpawnedInSession property + not a quest item
                return (item.Upd?.SpawnedInSession ?? false) &&
                       !(dbItems[(MongoId) item.Template].Properties.QuestItem ?? false) &&
                       !(
                           _inRaidConfig.KeepFiRSecureContainerOnDeath &&
                           _itemHelper.ItemIsInsideContainer(item, "SecuredContainer", items)
                       );
            }
        );

        foreach (var item in itemsToRemovePropertyFrom)
        {
            if (item.Upd is not null)
            {
                item.Upd.SpawnedInSession = false;
            }
        }
    }

    /// <summary>
    ///     Add items from one parameter into another.
    /// </summary>
    /// <param name="itemsToAdd">Items we want to add</param>
    /// <param name="serverInventoryItems">Location to add items to</param>
    protected void AddItemsToInventory(List<Item> itemsToAdd, List<Item> serverInventoryItems)
    {
        foreach (var itemToAdd in itemsToAdd)
        {
            // Try to find index of item to determine if we should add or replace
            var existingItemIndex = serverInventoryItems.FindIndex(
                inventoryItem => inventoryItem.Id == itemToAdd.Id
            );
            if (existingItemIndex == -1)
            {
                // Not found, add
                serverInventoryItems.Add(itemToAdd);
            }
            else
            {
                // Replace item with one from client
                serverInventoryItems.RemoveAt(existingItemIndex);
                serverInventoryItems.Add(itemToAdd);
            }
        }
    }

    /// <summary>
    ///     Clear PMC inventory of all items except those that are exempt.
    ///     Used post-raid to remove items after death.
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="sessionId">Session id</param>
    public void DeleteInventory(PmcData pmcData, string sessionId)
    {
        // Get inventory item ids to remove from players profile
        var itemIdsToDeleteFromProfile = GetInventoryItemsLostOnDeath(pmcData).Select(item => item.Id);
        foreach (var itemIdToDelete in itemIdsToDeleteFromProfile)
            // Items inside containers are handled as part of function
        {
            _inventoryHelper.RemoveItem(pmcData, itemIdToDelete, sessionId);
        }

        // Remove contents of fast panel
        pmcData.Inventory.FastPanel = new Dictionary<MongoId, MongoId>();
    }

    /// <summary>
    ///     Remove FiR status from designated container.
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="secureContainerSlotId">Container slot id to find items for and remove FiR from</param>
    public void RemoveFiRStatusFromItemsInContainer(
        string sessionId,
        PmcData pmcData,
        string secureContainerSlotId)
    {
        if (!pmcData.Inventory.Items.Any(item => item.SlotId == secureContainerSlotId))
        {
            return;
        }

        List<Item> itemsInsideContainer = [];
        foreach (var inventoryItem in pmcData.Inventory.Items.Where(item => item.Upd is not null && item.SlotId != "hideout"))
        {
            if (_itemHelper.ItemIsInsideContainer(inventoryItem, secureContainerSlotId, pmcData.Inventory.Items))
            {
                itemsInsideContainer.Add(inventoryItem);
            }
        }

        foreach (var item in itemsInsideContainer)
        {
            if (item.Upd.SpawnedInSession ?? false)
            {
                item.Upd.SpawnedInSession = false;
            }
        }
    }

    /// <summary>
    ///     Get a list of items from a profile that will be lost on death.
    /// </summary>
    /// <param name="pmcProfile">Profile to get items from</param>
    /// <returns>List of items lost on death</returns>
    protected List<Item> GetInventoryItemsLostOnDeath(PmcData pmcProfile)
    {
        var inventoryItems = pmcProfile.Inventory.Items ?? [];
        var equipmentRootId = pmcProfile?.Inventory?.Equipment;
        var questRaidItemContainerId = pmcProfile?.Inventory?.QuestRaidItems;

        return inventoryItems.Where(
                item =>
                {
                    // Keep items flagged as kept after death
                    if (IsItemKeptAfterDeath(pmcProfile, item))
                    {
                        return false;
                    }

                    // Remove normal items or quest raid items
                    if (item.ParentId == equipmentRootId || item.ParentId == questRaidItemContainerId)
                    {
                        return true;
                    }

                    // Pocket items are lost on death
                    // Ensure we dont pick up pocket items from manniquins
                    if (
                        item.SlotId.StartsWith("pocket") &&
                        _inventoryHelper.DoesItemHaveRootId(pmcProfile, item, pmcProfile.Inventory.Equipment)
                    )
                    {
                        return true;
                    }

                    return false;
                }
            )
            .ToList();
    }

    /// <summary>
    ///     Does the provided item's slotId mean it's kept on the player after death?
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="itemToCheck">Item to check should be kept</param>
    /// <returns>true if item is kept after death</returns>
    protected bool IsItemKeptAfterDeath(PmcData pmcData, Item itemToCheck)
    {
        // Base inventory items are always kept
        if (itemToCheck.ParentId is null)
        {
            return true;
        }

        // Is item equipped on player
        if (itemToCheck.ParentId == pmcData.Inventory.Equipment)
        {
            // Check slot id against config, true = delete, false = keep, undefined = delete
            var discard = _lostOnDeathConfig.Equipment.GetByJsonProp<bool>(itemToCheck.SlotId);
            if (discard)
                // Lost on death
            {
                return false;
            }

            return true;
        }

        // Should we keep items in pockets on death
        if (!_lostOnDeathConfig.Equipment.PocketItems && _pocketSlots.Contains(itemToCheck.SlotId))
        {
            return true;
        }

        // Is quest item + quest item not lost on death
        if (itemToCheck.ParentId == pmcData.Inventory.QuestRaidItems && !_lostOnDeathConfig.QuestItems)
        {
            return true;
        }

        // special slots are always kept after death
        if ((itemToCheck.SlotId?.Contains("SpecialSlot") ?? false) && _lostOnDeathConfig.SpecialSlotItems)
        {
            return true;
        }

        // All other cases item is lost
        return false;
    }
}
