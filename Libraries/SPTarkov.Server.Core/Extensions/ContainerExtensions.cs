using SPTarkov.Server.Core.Models.Spt.Inventory;

namespace SPTarkov.Server.Core.Extensions;

public static class ContainerExtensions
{
    /// <summary>
    ///     Finds a slot for an item in a given 2D container map
    /// </summary>
    /// <param name="container2D">List of container with positions filled/free</param>
    /// <param name="itemWidthX">Width of item</param>
    /// <param name="itemHeightY">Height of item</param>
    /// <returns>Location to place item in container</returns>
    public static FindSlotResult FindSlotForItem(this int[,] container2D, int? itemWidthX, int? itemHeightY)
    {
        // Assume not rotated
        var rotation = false;

        // Find the min volume the item will take up
        var minVolume = (itemWidthX < itemHeightY ? itemWidthX : itemHeightY) - 1;
        var containerY = container2D.GetLength(0); // rows
        var containerX = container2D.GetLength(1); // columns
        var limitY = containerY - minVolume;
        var limitX = containerX - minVolume;

        // Every x+y slot taken up in container, exit
        if (container2D.ContainerIsFull())
        {
            return new FindSlotResult(false);
        }

        // Down = y, iterate over rows
        for (var row = 0; row < limitY; row++)
        {
            if (RowIsFull(container2D, row))
            {
                continue;
            }

            // Left to right across columns, look for free position
            for (var column = 0; column < limitX; column++)
            {
                // Does item fit
                if (CanItemBePlacedInContainerAtPosition(container2D, row, column, itemWidthX.Value, itemHeightY.Value))
                {
                    // Success, found a spot it fits
                    return new FindSlotResult(true, column, row, rotation);
                }

                if (!ItemBiggerThan1X1(itemWidthX.Value, itemHeightY.Value))
                {
                    // Doesn't fit AND rotating won't help
                    continue;
                }

                // Rotate item by swapping x and y item values
                if (
                    CanItemBePlacedInContainerAtPosition(
                        container2D,
                        row,
                        column,
                        itemHeightY.Value, // Swapped
                        itemWidthX.Value // Swapped
                    )
                )
                {
                    // Found a position for the item when rotated
                    rotation = true;
                    return new FindSlotResult(true, column, row, rotation);
                }
            }
        }

        // Tried all possible positions, nothing big enough for item
        return new FindSlotResult(false);
    }

    /// <summary>
    ///     Find a free slot for an item to be placed at
    /// </summary>
    /// <param name="container2D">Container to place item in</param>
    /// <param name="columnStartPositionX">Container y size</param>
    /// <param name="rowStartPositionY">Container x size</param>
    /// <param name="itemXWidth">Items width</param>
    /// <param name="itemYHeight">Items height</param>
    /// <param name="isRotated">is item rotated</param>
    /// <param name="errorMessage">Error message if failed</param>
    /// <returns>bool = true when successful</returns>
    public static bool TryFillContainerMapWithItem(
        this int[,] container2D,
        int columnStartPositionX,
        int rowStartPositionY,
        int? itemXWidth,
        int? itemYHeight,
        bool isRotated,
        out string errorMessage
    )
    {
        errorMessage = string.Empty;

        var containerY = container2D.GetLength(0); // rows
        var containerX = container2D.GetLength(1); // columns

        // Swap height/width if item needs to be rotated to fit
        var itemWidth = isRotated ? itemYHeight : itemXWidth;
        var itemHeight = isRotated ? itemXWidth : itemYHeight;

        var itemRowEndPosition = rowStartPositionY + (itemHeight - 1);
        var itemColumnEndPosition = columnStartPositionX + (itemWidth - 1);

        //Item is a 1x1, flag slot as taken and exit early
        if (itemXWidth == 1 && itemYHeight == 1)
        {
            container2D[rowStartPositionY, columnStartPositionX] = 1;

            return true;
        }

        // Loop over rows and columns and flag each as taken by item
        for (var y = rowStartPositionY; y <= itemRowEndPosition; y++)
        {
            for (var x = columnStartPositionX; x <= itemColumnEndPosition; x++)
            {
                if (container2D[y, x] == 0)
                {
                    // Flag slot as used
                    container2D[y, x] = 1;
                }
                else
                {
                    errorMessage =
                        $"Slot at: ({containerX}, {containerY}) is already filled. Cannot fit: {itemXWidth} by {itemYHeight} item";
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Is the requested row full
    /// </summary>
    /// <param name="container2D">Container to check</param>
    /// <param name="rowIndex">Index of row to check</param>
    /// <returns>True = full</returns>
    private static bool RowIsFull(int[,] container2D, int rowIndex)
    {
        var rowFull = true;
        var containerColumnCount = container2D.GetLength(1); // Column
        for (var col = 0; col < containerColumnCount; col++)
        {
            if (container2D[rowIndex, col] == 0)
            {
                rowFull = false;
                break;
            }
        }

        return rowFull;
    }

    /// <summary>
    /// Is every slot in container full
    /// </summary>
    /// <param name="container2D">Container to check</param>
    /// <returns>True = full</returns>
    public static bool ContainerIsFull(this int[,] container2D)
    {
        var containerY = container2D.GetLength(0); // rows
        var containerX = container2D.GetLength(1); // columns
        var containerFull = true;
        for (var y = 0; y < containerY; y++)
        {
            for (var x = 0; x < containerX; x++)
            {
                if (container2D[y, x] == 0)
                {
                    containerFull = false;
                    break;
                }
            }
            if (!containerFull)
            {
                break;
            }
        }

        return containerFull;
    }

    /// <summary>
    /// Is the item size values passed in bigger than 1x1
    /// </summary>
    /// <param name="itemWidth">Width of item</param>
    /// <param name="itemHeight">Height of item</param>
    /// <returns>True = bigger than 1x1</returns>
    private static bool ItemBiggerThan1X1(int itemWidth, int itemHeight)
    {
        return itemWidth + itemHeight > 2;
    }

    /// <summary>
    ///     Can an item of specified size be placed inside a 2d container at a specific position
    /// </summary>
    /// <param name="container">Container to find space in</param>
    /// <param name="itemStartVerticalPos">Starting y position for item</param>
    /// <param name="itemStartHorizontalPos">Starting x position for item</param>
    /// <param name="itemWidth">Items width (y)</param>
    /// <param name="itemHeight">Items height (x)</param>
    /// <returns>True - slot found</returns>
    public static bool CanItemBePlacedInContainerAtPosition(
        this int[,] container,
        int itemStartVerticalPos,
        int itemStartHorizontalPos,
        int itemWidth,
        int itemHeight
    )
    {
        var containerHeight = container.GetLength(0); // Rows
        var containerWidth = container.GetLength(1); // Columns

        var itemEndColPosition = itemStartHorizontalPos + itemWidth - 1;
        var itemEndRowPosition = itemStartVerticalPos + itemHeight - 1;

        // Check item isn't bigger than container when at position
        if (itemEndColPosition > containerWidth - 1 || itemEndRowPosition > containerHeight - 1)
        {
            // Item is bigger than container, will never fit
            return false;
        }

        // Early exit if exact spot is taken
        if (container[itemStartVerticalPos, itemStartHorizontalPos] == 1)
        {
            return false;
        }

        // Single slot item, do direct check
        if (itemWidth == 1 && itemHeight == 1)
        {
            return container[itemStartVerticalPos, itemStartHorizontalPos] == 0;
        }

        for (var row = itemStartVerticalPos; row <= itemEndRowPosition; row++)
        {
            for (var column = itemStartHorizontalPos; column <= itemEndColPosition; column++)
            {
                if (container[row, column] == 1)
                {
                    // Occupied by something
                    return false;
                }
            }
        }

        return true; // Slot is free
    }
}
