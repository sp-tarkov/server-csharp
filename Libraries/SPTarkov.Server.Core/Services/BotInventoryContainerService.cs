using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Services;

/// <summary>
/// Service for keeping track of items and their exact position inside a bots container
/// </summary>
[Injectable]
public class BotInventoryContainerService(ISptLogger<BotGeneratorHelper> logger, ItemHelper itemHelper)
{
    // botId/containerName
    private readonly ConcurrentDictionary<MongoId, Dictionary<EquipmentSlots, ContainerDetails>> _botContainers = new();

    /// <summary>
    /// Add a container + details to a bots cache ready to accept loot
    /// </summary>
    /// <param name="botId">Unique identifier of bot</param>
    /// <param name="containerName">name of container e.g. "Backpack"</param>
    /// <param name="containerInventoryItem">Inventory item loot will be linked to in bots inventory</param>
    public void AddEmptyContainerToBot(MongoId botId, EquipmentSlots containerName, Item containerInventoryItem)
    {
        // Add bot to dict if it doesn't exist
        _botContainers.TryAdd(botId, new());

        // Get the bots' currently cached containers
        var containers = GetOrCreateBotContainerDictionary(botId);

        // Add container to bot
        if (!containers.ContainsKey(containerName))
        {
            var containerDbItem = itemHelper.GetItem(containerInventoryItem.Template);
            containers.Add(containerName, new ContainerDetails(containerDbItem.Value, containerInventoryItem));
        }
    }

    /// <summary>
    /// Attempt to add an item + children to a container
    /// </summary>
    /// <param name="botId">Bots unique id</param>
    /// <param name="containerName">Name of container to add to e.g. "Backpack"</param>
    /// <param name="itemAndChildren">Item and its children to add to container</param>
    /// <param name="botInventory">Inventory to add Item+children to</param>
    /// <param name="itemWidth">Width of item with its children</param>
    /// <param name="itemHeight">Height of item with its children</param>
    /// <returns>ItemAddedResult</returns>
    public ItemAddedResult AddItemToBotContainer(
        MongoId botId,
        EquipmentSlots containerName,
        List<Item> itemAndChildren,
        BotBaseInventory botInventory,
        int itemWidth,
        int itemHeight
    )
    {
        var addResult = ItemAddedResult.UNKNOWN;

        // Find bot and the container we will attempt to add into
        if (
            !GetOrCreateBotContainerDictionary(botId).TryGetValue(containerName, out var containerDetails)
            || containerDetails.ContainerGridDetails.Count == 0
        )
        {
            // No grids, cannot add item
            return ItemAddedResult.NO_CONTAINERS;
        }

        if (!ItemAllowedInContainer(containerDetails, itemAndChildren))
        // Multiple containers, maybe next one allows item, only break out of loop for the containers grids
        {
            return ItemAddedResult.INCOMPATIBLE_ITEM;
        }

        // Try to fit item into one of the containers' grids
        var rootItem = itemAndChildren.FirstOrDefault();
        var gridIndex = -1; // start at -1 as we increment index first thing each grid we iterate over
        foreach (var gridDb in containerDetails.ContainerDbItem.Properties.Grids)
        {
            gridIndex++;

            var gridDetails = containerDetails.ContainerGridDetails[gridIndex];
            if (gridDetails.GridFull)
            {
                // Skip to next grid
                continue;
            }

            if (IsItemBiggerThanGrid(gridDetails.GridMap, itemWidth, itemHeight))
            {
                // Skip to next grid
                continue;
            }

            // Look for a slot in the grid to place item
            var findSlotResult = gridDetails.GridMap.FindSlotForItem(itemWidth, itemHeight);
            if (findSlotResult.Success.GetValueOrDefault(false))
            {
                // It Fits!

                // Set items parent to Id of container
                if (rootItem is not null)
                {
                    rootItem.ParentId = containerDetails.ContainerInventoryItem.Id;
                    rootItem.SlotId = gridDb.Name; // Can be name of container e.g. "Backpack" OR "2/3/4/5" depending on which grid of a container item is added to
                    rootItem.Location = new ItemLocation
                    {
                        X = findSlotResult.X,
                        Y = findSlotResult.Y,
                        R = findSlotResult.Rotation ?? false ? ItemRotation.Vertical : ItemRotation.Horizontal,
                    };
                }

                // Flag result as success to report to caller
                addResult = ItemAddedResult.SUCCESS;

                // Update grid with slots taken up by above item
                FillGridRegion(
                    gridDetails.GridMap,
                    findSlotResult.X.Value,
                    findSlotResult.Y.Value,
                    findSlotResult.Rotation.GetValueOrDefault() ? itemHeight : itemWidth,
                    findSlotResult.Rotation.GetValueOrDefault() ? itemWidth : itemHeight
                );

                // Add item into bots inventory
                botInventory.Items.AddRange(itemAndChildren);

                // Exit loop, we've found a slot for item
                break;
            }

            // Didn't fit, flag as no space, hopefully next grid has space
            addResult = ItemAddedResult.NO_SPACE;

            // If item is 1x1 and it failed to fit, grid must be full
            if (itemHeight == 1 && itemWidth == 1)
            {
                gridDetails.GridFull = true; // Flag now so later items can skip grid
                continue;
            }

            // Check if grid is full and flag
            if (gridDetails.GridMap.ContainerIsFull())
            {
                gridDetails.GridFull = true;
            }
        }

        return addResult;
    }

    ///  <summary>
    /// Attempt to add an item + children to a container at a specific x/y grid position
    ///  </summary>
    ///  <param name="botId">Bots unique id</param>
    ///  <param name="containerName">Name of container to add to e.g. "Backpack"</param>
    ///  <param name="itemAndChildren">Item and its children to add to container</param>
    ///  <param name="botInventory">Inventory to add Item+children to</param>
    ///  <param name="itemWidth">Width of item with its children</param>
    ///  <param name="itemHeight">Height of item with its children</param>
    ///  <param name="fixedLocation">Details for where to place item in container grid</param>
    ///  <returns>ItemAddedResult</returns>
    public ItemAddedResult AddItemToBotContainerFixedPosition(
        MongoId botId,
        EquipmentSlots containerName,
        List<Item> itemAndChildren,
        BotBaseInventory botInventory,
        int itemWidth,
        int itemHeight,
        ItemLocation fixedLocation
    )
    {
        // Default result
        var addResult = ItemAddedResult.UNKNOWN;

        // Find bot and the container we are attempting to store item in
        var botContainers = GetOrCreateBotContainerDictionary(botId);

        botContainers.TryGetValue(containerName, out var containerDetails);

        if (containerDetails.ContainerGridDetails.Count == 0)
        {
            // No grids, cannot add item
            return ItemAddedResult.NO_CONTAINERS;
        }

        if (!ItemAllowedInContainer(containerDetails, itemAndChildren))
        // Multiple containers, maybe next one allows item, only break out of loop for the containers grids
        {
            return ItemAddedResult.INCOMPATIBLE_ITEM;
        }

        // Try to fit item into one of the containers' grids
        var rootItem = itemAndChildren.FirstOrDefault();
        if (rootItem is null)
        {
            return ItemAddedResult.UNKNOWN;
        }
        foreach (var gridDetails in containerDetails.ContainerGridDetails)
        {
            if (gridDetails.GridFull)
            {
                // No space, skip early
                continue;
            }

            if (IsItemBiggerThanGrid(gridDetails.GridMap, itemWidth, itemHeight))
            {
                // Skip early
                continue;
            }

            // Look for a slot in the grid to place item
            var result = gridDetails.GridMap.TryFillContainerMapWithItem(
                fixedLocation.X.Value,
                fixedLocation.Y.Value,
                itemWidth,
                itemHeight,
                fixedLocation.R == ItemRotation.Vertical,
                out _
            );
            if (result)
            {
                // It Fits!

                // Parent root item to container
                rootItem.ParentId = containerDetails.ContainerInventoryItem.Id;
                rootItem.SlotId = containerName.ToString();
                rootItem.Location = new ItemLocation
                {
                    X = fixedLocation.X.Value,
                    Y = fixedLocation.Y.Value,
                    R = fixedLocation.R,
                };

                // Flag result as success to report to caller
                addResult = ItemAddedResult.SUCCESS;

                // Update internal grid with slots taken up by above item
                FillGridRegion(
                    gridDetails.GridMap,
                    fixedLocation.X.Value,
                    fixedLocation.Y.Value,
                    fixedLocation.R == ItemRotation.Vertical ? itemHeight : itemWidth,
                    fixedLocation.R == ItemRotation.Vertical ? itemWidth : itemHeight
                );

                // Item fits + Added to layout grid, add item and children
                //containerDetails.ItemsAndChildrenInContainer.AddRange(itemAndChildren);

                // Add item into bots inventory
                botInventory.Items.AddRange(itemAndChildren);

                // Exit loop, we've found a position for item and can stop
                break;
            }

            // Didn't fit, flag as no space, hopefully next grid has space
            addResult = ItemAddedResult.NO_SPACE;

            // If the item is 1x1 and it failed to fit, grid must be full
            if (itemHeight == 1 && itemWidth == 1)
            {
                gridDetails.GridFull = true;
                continue;
            }

            // Check if grid is full and flag
            if (gridDetails.GridMap.ContainerIsFull())
            {
                gridDetails.GridFull = true;
            }
        }

        return addResult;
    }

    /// <summary>
    /// Helper - Get the bot-specific container details, create if data doesn't exist
    /// </summary>
    /// <param name="botId">Bot unique identifier</param>
    /// <returns>Dictionary</returns>
    protected Dictionary<EquipmentSlots, ContainerDetails> GetOrCreateBotContainerDictionary(MongoId botId)
    {
        if (!_botContainers.TryGetValue(botId, out var botContainers))
        {
            // Create blank dict ready for containers to be added
            botContainers = new();
        }

        return botContainers;
    }

    /// <summary>
    /// Fill region of a 2D array
    /// </summary>
    /// <param name="grid">The 2D grid array to modify</param>
    /// <param name="x">The starting column index (left)</param>
    /// <param name="y">The starting row index (top)</param>
    /// <param name="itemWidth">The number of cells to update horizontally</param>
    /// <param name="itemHeight">The number of cells to update vertically</param>
    private void FillGridRegion(int[,] grid, int x, int y, int itemWidth, int itemHeight)
    {
        // Outer loop iterates through rows (from starting y position)
        for (var row = y; row < y + itemHeight; row++)
        {
            // Inner loop iterates through columns (from starting x position)
            for (var col = x; col < x + itemWidth; col++)
            {
                grid[row, col] = 1;
            }
        }
    }

    /// <summary>
    /// Is the items subtype allowed inside this container / is it excluded from this container
    /// </summary>
    /// <param name="containerDetails">Details on the container we want to add item into</param>
    /// <param name="itemAndChildren">Item+children we want to add into container</param>
    /// <returns>true = item is allowed</returns>
    private bool ItemAllowedInContainer(ContainerDetails containerDetails, List<Item>? itemAndChildren)
    {
        // Assume all grids have same limitations
        var firstSlotGrid = containerDetails.ContainerDbItem.Properties.Grids.FirstOrDefault();
        var propFilters = firstSlotGrid?.Props?.Filters;
        if (propFilters is null || !propFilters.Any())
        // No filters, item is fine to add
        {
            return true;
        }

        // Check if item base type is excluded
        var itemDetails = itemHelper.GetItem(itemAndChildren.FirstOrDefault().Template).Value;

        // if item to add is found in exclude filter, not allowed
        var excludedFilter = propFilters.FirstOrDefault()?.ExcludedFilter ?? [];
        if (excludedFilter.Contains(itemDetails?.Parent ?? string.Empty))
        {
            return false;
        }

        // If Filter array only contains 1 filter and it is for basetype 'item', allow it
        var filter = propFilters.FirstOrDefault()?.Filter ?? [];
        if (filter.Count == 1 && filter.Contains(BaseClasses.ITEM))
        {
            return true;
        }

        // If allowed filter has something in it + filter doesn't have basetype 'item', not allowed
        if (filter.Count > 0 && !filter.Contains(itemDetails?.Parent ?? string.Empty))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Is the items edge length bigger than the grid trying to hold it
    /// </summary>
    /// <param name="grid">Container grid</param>
    /// <param name="itemWidth">Width of item</param>
    /// <param name="itemHeight">Height of item</param>
    /// <returns>true = item bigger than grid</returns>
    private bool IsItemBiggerThanGrid(int[,] grid, int itemWidth, int itemHeight)
    {
        var gridHeight = grid.GetLength(0);
        var gridWidth = grid.GetLength(1);

        // Check if it can fit in either orientation
        var fitsNormally = itemWidth <= gridWidth && itemHeight <= gridHeight;
        var fitsRotated = itemHeight <= gridWidth && itemWidth <= gridHeight;

        // Fails both checks
        return !fitsNormally && !fitsRotated;
    }

    /// <summary>
    /// Get a bots container details from cache by its id
    /// </summary>
    /// <param name="botId">Identifier of bot to get details of</param>
    /// <returns>Dictionary of containers and their details</returns>
    public Dictionary<EquipmentSlots, ContainerDetails>? GetBotContainer(MongoId botId)
    {
        return GetOrCreateBotContainerDictionary(botId);
    }

    /// <summary>
    ///  Clear the cache of all bot containers
    /// </summary>
    public void ClearCache()
    {
        _botContainers.Clear();
    }

    /// <summary>
    /// Clear specific bot container details from cache
    /// </summary>
    /// <param name="botId">Bot identifier</param>
    public void ClearCache(MongoId botId)
    {
        _botContainers.Remove(botId, out _);
    }

    public record ContainerDetails
    {
        public ContainerDetails(TemplateItem containerDbItem, Item containerInventoryItem)
        {
            ContainerDbItem = containerDbItem;
            ContainerInventoryItem = containerInventoryItem;
            // Add all grids for this container
            foreach (var grid in containerDbItem.Properties.Grids)
            {
                ContainerGridDetails.Add(
                    new ContainerMapDetails
                    {
                        GridMap = new int[grid.Props.CellsV.GetValueOrDefault(), grid.Props.CellsH.GetValueOrDefault()],
                        GridFull = false,
                    }
                );
            }
        }

        /// <summary>
        /// Grid layout and flag if grid is full
        /// </summary>
        public List<ContainerMapDetails> ContainerGridDetails { get; } = [];

        /// <summary>
        /// Db record for the container holding items
        /// </summary>
        public TemplateItem ContainerDbItem { get; set; }

        /// <summary>
        /// Inventory item representing the container
        /// </summary>
        public Item ContainerInventoryItem { get; set; }

        // TODO: implement this + add checks inside AddItemToBotContainer for perf improvement
        public bool ContainerFull { get; set; } = false;
    }

    public record ContainerMapDetails
    {
        public int[,] GridMap { get; init; }
        public bool GridFull { get; set; }
    }
}
