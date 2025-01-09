using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;

namespace Core.Helpers;

public class InRaidHelper
{
    /// <summary>
    /// Deprecated. Reset the skill points earned in a raid to 0, ready for next raid.
    /// </summary>
    /// <param name="profile">Profile to update</param>
    protected void ResetSkillPointsEarnedDuringRaid(PmcData profile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update a player's inventory post-raid.
    /// Remove equipped items from pre-raid.
    /// Add new items found in raid to profile.
    /// Store insurance items in profile.
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Remove FiR status from items.
    /// </summary>
    /// <param name="items">Items to process</param>
    protected void RemoveFiRStatusFromCertainItems(List<Item> items)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add items from one parameter into another.
    /// </summary>
    /// <param name="itemsToAdd">Items we want to add</param>
    /// <param name="serverInventoryItems">Location to add items to</param>
    protected void AddItemsToInventory(List<Item> itemsToAdd, List<Item> serverInventoryItems)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Clear PMC inventory of all items except those that are exempt.
    /// Used post-raid to remove items after death.
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="sessionId">Session id</param>
    public void DeleteInventory(PmcData pmcData, string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Remove FiR status from designated container.
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="secureContainerSlotId">Container slot id to find items for and remove FiR from</param>
    public void RemoveFiRStatusFromItemsInContainer(
        string sessionId,
        PmcData pmcData,
        string secureContainerSlotId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a list of items from a profile that will be lost on death.
    /// </summary>
    /// <param name="pmcProfile">Profile to get items from</param>
    /// <returns>List of items lost on death</returns>
    protected List<Item> GetInventoryItemsLostOnDeath(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Does the provided item's slotId mean it's kept on the player after death?
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="itemToCheck">Item to check should be kept</param>
    /// <returns>true if item is kept after death</returns>
    protected bool IsItemKeptAfterDeath(PmcData pmcData, Item itemToCheck)
    {
        throw new NotImplementedException();
    }
}
