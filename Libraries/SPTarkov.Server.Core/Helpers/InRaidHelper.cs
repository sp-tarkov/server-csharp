using System.Collections.Frozen;
using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class InRaidHelper(InventoryHelper inventoryHelper, ConfigServer configServer, ICloner cloner, DatabaseService databaseService)
{
    protected static readonly FrozenSet<string> _pocketSlots = ["pocket1", "pocket2", "pocket3", "pocket4"];
    protected readonly InRaidConfig _inRaidConfig = configServer.GetConfig<InRaidConfig>();
    protected readonly LostOnDeathConfig _lostOnDeathConfig = configServer.GetConfig<LostOnDeathConfig>();

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
    public void SetInventory(MongoId sessionID, PmcData serverProfile, PmcData postRaidProfile, bool isSurvived, bool isTransfer)
    {
        // Store insurance (as removeItem() removes insured items)
        var insured = cloner.Clone(serverProfile.InsuredItems);

        // Remove equipment and loot items stored on player from server profile in preparation for data from client being added
        inventoryHelper.RemoveItem(serverProfile, serverProfile.Inventory.Equipment.Value, sessionID);

        // Remove quest items stored on player from server profile in preparation for data from client being added
        inventoryHelper.RemoveItem(serverProfile, serverProfile.Inventory.QuestRaidItems.Value, sessionID);

        // Get all items that have a parent of `serverProfile.Inventory.equipment` (All items player had on them at end of raid)
        var postRaidInventoryItems = postRaidProfile.Inventory.Items.GetItemWithChildren(postRaidProfile.Inventory.Equipment.Value);

        // Get all items that have a parent of `serverProfile.Inventory.questRaidItems` (Quest items player had on them at end of raid)
        var postRaidQuestItems = postRaidProfile.Inventory.Items.GetItemWithChildren(postRaidProfile.Inventory.QuestRaidItems.Value);

        // Handle Removing of FIR status if player did not survive + not transferring
        // Do after above filtering code to reduce work done
        if (!isSurvived && !isTransfer && !_inRaidConfig.AlwaysKeepFoundInRaidOnRaidEnd)
        {
            RemoveFiRStatusFromItems(postRaidProfile.Inventory.Items);
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
    protected void RemoveFiRStatusFromItems(IEnumerable<Item> items)
    {
        var dbItems = databaseService.GetItems();

        var itemsToRemovePropertyFrom = items.Where(item =>
        {
            // Has upd object + upd.SpawnedInSession property + not a quest item
            return (item.Upd?.SpawnedInSession ?? false)
                && !(dbItems[item.Template].Properties.QuestItem ?? false)
                && !(_inRaidConfig.KeepFiRSecureContainerOnDeath && item.ItemIsInsideContainer("SecuredContainer", items));
        });

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
    protected void AddItemsToInventory(IEnumerable<Item> itemsToAdd, List<Item> serverInventoryItems)
    {
        foreach (var itemToAdd in itemsToAdd)
        {
            // Try to find index of item to determine if we should add or replace
            var existingItemIndex = serverInventoryItems.FindIndex(inventoryItem => inventoryItem.Id == itemToAdd.Id);
            if (existingItemIndex != -1)
            {
                // Replace existing item
                serverInventoryItems.RemoveAt(existingItemIndex);
            }

            // Add new item
            serverInventoryItems.Add(itemToAdd);
        }
    }

    /// <summary>
    ///     Clear PMC inventory of all items except those that are exempt.
    ///     Used post-raid to remove items after death.
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="sessionId">Player/Session id</param>
    public void DeleteInventory(PmcData pmcData, MongoId sessionId)
    {
        // Get inventory items to remove from players profile
        var itemsToDeleteFromProfile = GetInventoryItemsLostOnDeath(pmcData).ToList();

        foreach (var itemToDelete in itemsToDeleteFromProfile)
        {
            // Items inside containers are handled as part of function
            inventoryHelper.RemoveItem(pmcData, itemToDelete.Id, sessionId);
        }

        // Remove contents of fast panel
        pmcData.Inventory.FastPanel = new();
    }

    /// <summary>
    ///     Get a list of items from a profile that will be lost on death.
    /// </summary>
    /// <param name="pmcProfile">Profile to get items from</param>
    /// <returns>List of items lost on death</returns>
    protected IEnumerable<Item> GetInventoryItemsLostOnDeath(PmcData pmcProfile)
    {
        var inventoryItems = pmcProfile.Inventory.Items ?? [];
        var equipmentRootId = pmcProfile?.Inventory?.Equipment;
        var questRaidItemContainerId = pmcProfile?.Inventory?.QuestRaidItems;

        return inventoryItems.Where(item =>
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
            // Ensure we don't pick up pocket items from mannequins
            if (item.SlotId.StartsWith("pocket") && pmcProfile.DoesItemHaveRootId(item, pmcProfile.Inventory.Equipment.Value))
            {
                return true;
            }

            return false;
        });
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
