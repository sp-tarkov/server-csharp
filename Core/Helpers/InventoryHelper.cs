using System.Text.Json.Serialization;
using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Inventory;
using Core.Models.Eft.ItemEvent;
using Core.Models.Spt.Config;
using Core.Models.Spt.Inventory;

namespace Core.Helpers;

[Injectable]
public class InventoryHelper
{
    /// <summary>
    /// Add multiple items to player stash (assuming they all fit)
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="request">AddItemsDirectRequest request</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="output">Client response object</param>
    public void AddItemsToStash(
        string sessionId,
        AddItemsDirectRequest request,
        PmcData pmcData,
        ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add whatever is passed in request.itemWithModsToAdd into player inventory (if it fits)
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="request">AddItemDirect request</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="output">Client response object</param>
    public void AddItemToStash(
        string sessionId,
        AddItemDirectRequest request,
        PmcData pmcData,
        ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Set FiR status for an item + its children
    /// </summary>
    /// <param name="itemWithChildren">An item</param>
    /// <param name="foundInRaid">Item was found in raid</param>
    protected void SetFindInRaidStatusForItem(Item[] itemWithChildren, bool foundInRaid)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Remove properties from a Upd object used by a trader/ragfair that are unnecessary to a player
    /// </summary>
    /// <param name="upd">Object to update</param>
    protected void RemoveTraderRagfairRelatedUpdProperties(Upd upd)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Can all provided items be added into player inventory
    /// </summary>
    /// <param name="sessionId">Player id</param>
    /// <param name="itemsWithChildren">Array of items with children to try and fit</param>
    /// <returns>True all items fit</returns>
    public bool CanPlaceItemsInInventory(string sessionId, List<List<Item>> itemsWithChildren)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Do the provided items all fit into the grid
    /// </summary>
    /// <param name="containerFS2D">Container grid to fit items into</param>
    /// <param name="itemsWithChildren">Items to try and fit into grid</param>
    /// <returns>True all fit</returns>
    public bool CanPlaceItemsInContainer(List<List<double>>? containerFS2D, List<List<Item>> itemsWithChildren)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Does an item fit into a container grid
    /// </summary>
    /// <param name="containerFS2D">Container grid</param>
    /// <param name="itemWithChildren">Item to check fits</param>
    /// <returns>True it fits</returns>
    public bool CanPlaceItemInContainer(List<List<int>>? containerFS2D, List<Item> itemWithChildren)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find a free location inside a container to fit the item
    /// </summary>
    /// <param name="containerFS2D">Container grid to add item to</param>
    /// <param name="itemWithChildren">Item to add to grid</param>
    /// <param name="containerId">Id of the container we're fitting item into</param>
    /// <param name="desiredSlotId">Slot id value to use, default is "hideout"</param>
    public void PlaceItemInContainer(
        List<List<double>> containerFS2D,
        List<Item> itemWithChildren,
        string containerId,
        string desiredSlotId = "hideout")
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find a location to place an item into inventory and place it
    /// </summary>
    /// <param name="stashFS2D">2-dimensional representation of the container slots</param>
    /// <param name="sortingTableFS2D">2-dimensional representation of the sorting table slots</param>
    /// <param name="itemWithChildren">Item to place with children</param>
    /// <param name="playerInventory">Players inventory</param>
    /// <param name="useSortingTable">Should sorting table to be used if main stash has no space</param>
    /// <param name="output">Output to send back to client</param>
    protected void PlaceItemInInventory(
        List<List<double>> stashFS2D,
        List<List<double>> sortingTableFS2D,
        List<Item> itemWithChildren,
        BotBaseInventory playerInventory,
        bool useSortingTable,
        ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle Remove event
    /// Remove item from player inventory + insured items array
    /// Also deletes child items
    /// </summary>
    /// <param name="profile">Profile to remove item from (pmc or scav)</param>
    /// <param name="itemId">Items id to remove</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="output">OPTIONAL - ItemEventRouterResponse</param>
    public void RemoveItem(PmcData profile, string itemId, string sessionId, ItemEventRouterResponse output = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete desired item from a player profiles mail
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="removeRequest">Remove request</param>
    /// <param name="output">OPTIONAL - ItemEventRouterResponse</param>
    public void RemoveItemAndChildrenFromMailRewards(string sessionId, InventoryRemoveRequestData removeRequest, ItemEventRouterResponse output = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find item by id in player inventory and remove x of its count
    /// </summary>
    /// <param name="pmcData">player profile</param>
    /// <param name="itemId">Item id to decrement StackObjectsCount of</param>
    /// <param name="countToRemove">Number of item to remove</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="output">ItemEventRouterResponse</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse RemoveItemByCount(PmcData pmcData, string itemId, int countToRemove, string sessionId, ItemEventRouterResponse output = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the height and width of an item - can have children that alter size
    /// </summary>
    /// <param name="itemTpl">Item to get size of</param>
    /// <param name="itemId">Items id to get size of</param>
    /// <param name="inventoryItems"></param>
    /// <returns>[width, height]</returns>
    public List<int> GetItemSize(string? itemTpl, string itemId, List<Item> inventoryItems)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Calculates the size of an item including attachments
    /// takes into account if item is folded
    /// </summary>
    /// <param name="itemTpl">Items template id</param>
    /// <param name="itemId">Items id</param>
    /// <param name="inventoryItemHash">Hashmap of inventory items</param>
    /// <returns>An array representing the [width, height] of the item</returns>
    protected List<int> GetSizeByInventoryItemHash(string itemTpl, string itemId, InventoryItemHash inventoryItemHash)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a blank two-dimensional representation of a container
    /// </summary>
    /// <param name="containerH">Horizontal size of container</param>
    /// <param name="containerY">Vertical size of container</param>
    /// <returns>Two-dimensional representation of container</returns>
    protected List<List<int>> GetBlankContainerMap(int containerH, int containerY)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a 2d mapping of a container with what grid slots are filled
    /// </summary>
    /// <param name="containerH">Horizontal size of container</param>
    /// <param name="containerV">Vertical size of container</param>
    /// <param name="itemList">Players inventory items</param>
    /// <param name="containerId">Id of the container</param>
    /// <returns>Two-dimensional representation of container</returns>
    public List<List<int>> GetContainerMap(double containerH, double containerV, List<Item> itemList, string containerId)
    {
        throw new NotImplementedException();
    }

    protected bool IsVertical(ItemLocation itemLocation)
    {
        throw new NotImplementedException();
    }

    protected InventoryItemHash GetInventoryItemHash(List<Item> inventoryItems)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the inventory that needs to be modified (scav/pmc etc)
    /// Changes made to result apply to character inventory
    /// Based on the item action, determine whose inventories we should be looking at for from and to.
    /// </summary>
    /// <param name="request">Item interaction request</param>
    /// <param name="sessionId">Session id / playerid</param>
    /// <returns>OwnerInventoryItems with inventory of player/scav to adjust</returns>
    public OwnerInventoryItems GetOwnerInventoryItems(
        InventoryMoveRequestData request,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a two dimensional array to represent stash slots
    /// 0 value = free, 1 = taken
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="sessionID">session id</param>
    /// <returns>2-dimensional array</returns>
    protected int[,] GetStashSlotMap(PmcData pmcData, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a blank two-dimensional array representation of a container
    /// </summary>
    /// <param name="containerTpl">Container to get data for</param>
    /// <returns>blank two-dimensional array</returns>
    public List<List<double>> GetContainerSlotMap(string containerTpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a two-dimensional array representation of the players sorting table
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <returns>two-dimensional array</returns>
    protected List<List<double>> GetSortingTableSlotMap(PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get Players Stash Size
    /// </summary>
    /// <param name="sessionID">Players id</param>
    /// <returns>Dictionary of 2 values, horizontal and vertical stash size</returns>
    protected Dictionary<double, double> GetPlayerStashSize(string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the players stash items tpl
    /// </summary>
    /// <param name="sessionID">Player id</param>
    /// <returns>Stash tpl</returns>
    protected string GetStashType(string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Internal helper function to transfer an item + children from one profile to another.
    /// </summary>
    /// <param name="sourceItems">Inventory of the source (can be non-player)</param>
    /// <param name="toItems">Inventory of the destination</param>
    /// <param name="request">Move request</param>
    public void MoveItemToProfile(Item[] sourceItems, Item[] toItems, InventoryMoveRequestData request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Internal helper function to move item within the same profile.
    /// </summary>
    /// <param name="pmcData">profile to edit</param>
    /// <param name="inventoryItems"></param>
    /// <param name="moveRequest">client move request</param>
    /// <returns>True if move was successful</returns>
    public (bool success, string errorMessage) MoveItemInternal(
        PmcData pmcData,
        Item[] inventoryItems,
        InventoryMoveRequestData moveRequest)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update fast panel bindings when an item is moved into a container that doesn't allow quick slot access
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="itemBeingMoved">item being moved</param>
    protected void UpdateFastPanelBinding(PmcData pmcData, Item itemBeingMoved)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Internal helper function to handle cartridges in inventory if any of them exist.
    /// </summary>
    protected void HandleCartridges(Item[] items, InventoryMoveRequestData request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get details for how a random loot container should be handled, max rewards, possible reward tpls
    /// </summary>
    /// <param name="itemTpl">Container being opened</param>
    /// <returns>Reward details</returns>
    public RewardDetails GetRandomLootContainerRewardDetails(string itemTpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get inventory configuration
    /// </summary>
    /// <returns>Inventory configuration</returns>
    public InventoryConfig GetInventoryConfig()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Recursively checks if the given item is
    /// inside the stash, that is it has the stash as
    /// ancestor with slotId=hideout
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="itemToCheck">Item to look for</param>
    /// <returns>True if item exists inside stash</returns>
    public bool IsItemInStash(PmcData pmcData, Item itemToCheck)
    {
        throw new NotImplementedException();
    }

    public void ValidateInventoryUsesMongoIds(List<Item> itemsToValidate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Does the provided item have a root item with the provided id
    /// </summary>
    /// <param name="pmcData">Profile with items</param>
    /// <param name="item">Item to check</param>
    /// <param name="rootId">Root item id to check for</param>
    /// <returns>True when item has rootId, false when not</returns>
    public bool DoesItemHaveRootId(PmcData pmcData, Item item, string rootId)
    {
        throw new NotImplementedException();
    }
}

public class InventoryItemHash
{
    [JsonPropertyName("byItemId")]
    public Dictionary<string, Item> ByItemId { get; set; }

    [JsonPropertyName("byParentId")]
    public Dictionary<string, List<Item>> ByParentId { get; set; }
}
