using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotInventoryContainerService(ISptLogger<BotGeneratorHelper> logger, ItemHelper itemHelper)
{
    // botId/containerName
    private static readonly Dictionary<MongoId, Dictionary<EquipmentSlots, ContainerDetails>> _botContainers = new();

    public void AddEmptyContainerToBot(MongoId botId, EquipmentSlots containerId, TemplateItem containerDbItem, Item containerInventoryItem)
    {
        // Add bot to dict if it doesn't exist
        _botContainers.TryAdd(botId, new());

        // Get the bots' currently cached containers
        if (!_botContainers.TryGetValue(botId, out var containers))
        {
            // Create blank dict of items against container id
            containers = new();
        }

        // Add container to bot
        if (!containers.TryGetValue(containerId, out var itemsInContainer))
        {
            containers.Add(containerId, new ContainerDetails(containerDbItem, containerInventoryItem));
        }
    }

    /// <summary>
    /// Attempt to add an item + children to a container
    /// </summary>
    /// <param name="botId">Bots unique id</param>
    /// <param name="containerName">Name of container to add to e.g. "Backpack"</param>
    /// <param name="itemAndChildren">Item and its children to add to container</param>
    /// <param name="itemWidth">Width of item with its children</param>
    /// <param name="itemHeight">Height of item with its children</param>
    /// <returns>ItemAddedResult</returns>
    public ItemAddedResult AddItemToBotContainer(
        MongoId botId,
        EquipmentSlots containerName,
        List<Item> itemAndChildren,
        int itemWidth,
        int itemHeight
    )
    {
        var addResult = ItemAddedResult.UNKNOWN;

        // Find bot and the container we are will attempt to add to
        _botContainers.TryGetValue(botId, out var botContainers);
        botContainers.TryGetValue(containerName, out var containerDetails);

        if (containerDetails.ContainerGridDetails.Count == 0)
        {
            // No grids, cannot add item
            return ItemAddedResult.NO_CONTAINERS;
        }

        // Try to fit item into one of the containers grids
        var rootItem = itemAndChildren.FirstOrDefault();
        foreach (var gridDetails in containerDetails.ContainerGridDetails)
        {
            if (gridDetails.GridFull)
            {
                continue;
            }

            if (IsGridSmallerThanItem(gridDetails.GridMap, itemWidth, itemHeight))
            {
                // Skip to next grid
                continue;
            }

            // TODO: move out of loop - if it fails one, it'll probably fail all grids
            if (!ItemAllowedInContainer(containerDetails, itemAndChildren))
            // Multiple containers, maybe next one allows item, only break out of loop for the containers grids
            {
                break;
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
                    rootItem.SlotId = containerName.ToString();
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

                // Item fits + Added to layout grid, add item and children
                containerDetails.ItemsAndChildrenInContainer.AddRange(itemAndChildren);

                // Exit loop, we've found a slot for item
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
    /// Fill region of a 2D array
    /// </summary>
    /// <param name="grid">The 2D integer array to modify</param>
    /// <param name="x">The starting column index (left)</param>
    /// <param name="y">The starting row index (top)</param>
    /// <param name="itemWidth">The number of cells to update horizontally</param>
    /// <param name="itemHeight">The number of cells to update vertically</param>
    private void FillGridRegion(int[,] grid, int x, int y, int itemWidth, int itemHeight)
    {
        // --- Update Logic ---
        // Iterate through the specified rectangular region and set the value to 1.
        // The outer loop iterates through the rows (from the starting y position).
        for (var row = y; row < y + itemHeight; row++)
        {
            // The inner loop iterates through the columns (from the starting x position).
            for (var col = x; col < x + itemWidth; col++)
            {
                grid[row, col] = 1;
            }
        }
    }

    /// <summary>
    /// Is the items subtype allowed inside this container
    /// </summary>
    /// <param name="containerDetails"></param>
    /// <param name="rootItem"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private bool ItemAllowedInContainer(ContainerDetails containerDetails, List<Item>? rootItem)
    {
        // Assume all grids have same limitations
        // TODO
        var firstSlotGrid = containerDetails.ContainerDbItem.Properties.Grids.FirstOrDefault();
        return true;
    }

    /// <summary>
    /// Is the items edge length bigger than the grid trying to hold it
    /// </summary>
    /// <param name="map"></param>
    /// <param name="itemWidth"></param>
    /// <param name="itemHeight"></param>
    /// <returns></returns>
    private bool IsGridSmallerThanItem(int[,] map, int itemWidth, int itemHeight)
    {
        return itemWidth * itemHeight > map.GetLength(0) * map.GetLength(1);
    }

    public ContainerDetails? GetBotContainerDetails(MongoId botId, EquipmentSlots containerName)
    {
        _botContainers.TryGetValue(botId, out var containers);

        return containers.GetValueOrDefault(containerName);
    }

    public List<List<Item>> GetItemsInContainer(MongoId botId, EquipmentSlots containerName)
    {
        var details = GetBotContainerDetails(botId, containerName);
        return details.ItemsAndChildrenInContainer;
    }

    public void ClearCache()
    {
        _botContainers.Clear();
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

        public List<List<Item>> ItemsAndChildrenInContainer { get; } = [];
        public List<ContainerMapDetails> ContainerGridDetails { get; } = [];
        public TemplateItem ContainerDbItem { get; set; }

        /// <summary>
        /// Inventory item representing the container
        /// </summary>
        public Item ContainerInventoryItem { get; set; }
        public bool ContainerFull { get; set; } = false;
    }

    public record ContainerMapDetails
    {
        public int[,] GridMap { get; set; }
        public bool GridFull { get; set; }
    }
}
