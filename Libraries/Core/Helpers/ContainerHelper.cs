using System.Text.Json.Serialization;
using SptCommon.Annotations;

namespace Core.Helpers;

[Injectable]
public class ContainerHelper
{
    /// <summary>
    ///     Finds a slot for an item in a given 2D container map
    /// </summary>
    /// <param name="container2D">List of container with slots filled/free</param>
    /// <param name="itemWidth">Width of item</param>
    /// <param name="itemHeight">Height of item</param>
    /// <returns>Location to place item in container</returns>
    public FindSlotResult FindSlotForItem(int[][] container2D, int itemWidth, int itemHeight)
    {
        var rotation = false;
        var minVolume = (itemWidth < itemHeight ? itemWidth : itemHeight) - 1;
        var containerY = container2D.Length;
        var containerX = container2D[0].Length;
        var limitY = containerY - minVolume;
        var limitX = containerX - minVolume;

        // Every x+y slot taken up in container, exit
        if (container2D.All(x => x.All(y => y == 1)))
        {
            return new FindSlotResult(false);
        }

        // Down = y
        for (var y = 0; y < limitY; y++)
        {
            if (container2D[y].All(x => x == 1))
                // Every item in row is full, skip row
            {
                continue;
            }

            // Try each slot on the row (across = x)
            for (var x = 0; x < limitX; x++)
            {
                var foundSlot = LocateSlot(container2D, containerX, containerY, x, y, itemWidth, itemHeight);
                if (foundSlot)
                {
                    return new FindSlotResult(true, x, y, rotation);
                }

                // Failed to find slot, rotate item and try again
                if (!foundSlot && ItemBiggerThan1X1(itemWidth, itemHeight))
                {
                    // Bigger than 1x1, try rotating
                    foundSlot = LocateSlot(container2D, containerX, containerY, x, y, itemHeight, itemWidth); // Height/Width swapped
                    if (foundSlot)
                    {
                        // Found a slot for it when rotated
                        rotation = true;

                        return new FindSlotResult(true, x, y, rotation);
                    }
                }
            }
        }

        // Tried all possible holes, nothing big enough for the item
        return new FindSlotResult(false);
    }

    protected static bool ItemBiggerThan1X1(int itemWidth, int itemHeight)
    {
        return itemWidth * itemHeight > 1;
    }

    /// <summary>
    ///     Find a slot inside a container an item can be placed in
    /// </summary>
    /// <param name="container2D">Container to find space in</param>
    /// <param name="containerX">Container x size</param>
    /// <param name="containerY">Container y size</param>
    /// <param name="x">???</param>
    /// <param name="y">???</param>
    /// <param name="itemW">Items width</param>
    /// <param name="itemH">Items height</param>
    /// <returns>True - slot found</returns>
    protected bool LocateSlot(
        int[][] container2D,
        int containerX,
        int containerY,
        int x,
        int y,
        int itemW,
        int itemH)
    {
        var foundSlot = true;

        for (var itemY = 0; itemY < itemH; itemY++)
        {
            if (foundSlot && y + itemH - 1 > containerY - 1)
            {
                foundSlot = false;
                break;
            }

            // Does item fit x-ways across
            for (var itemX = 0; itemX < itemW; itemX++)
            {
                if (foundSlot && x + itemW - 1 > containerX - 1)
                {
                    foundSlot = false;
                    break;
                }

                if (container2D[y + itemY][x + itemX] != 0)
                {
                    foundSlot = false;
                    break;
                }
            }

            if (!foundSlot)
            {
                break;
            }
        }

        return foundSlot;
    }

    /// <summary>
    ///     Find a free slot for an item to be placed at
    /// </summary>
    /// <param name="container2D">Container to place item in</param>
    /// <param name="x">Container x size</param>
    /// <param name="y">Container y size</param>
    /// <param name="itemW">Items width</param>
    /// <param name="itemH">Items height</param>
    /// <param name="rotate">is item rotated</param>
    public void FillContainerMapWithItem(
        int[][] container2D,
        int x,
        int y,
        int itemW,
        int itemH,
        bool rotate)
    {
        // Swap height/width if we want to fit it in rotated
        var itemWidth = rotate ? itemH : itemW;
        var itemHeight = rotate ? itemW : itemH;

        for (var tmpY = y; tmpY < y + itemHeight; tmpY++)
        for (var tmpX = x; tmpX < x + itemWidth; tmpX++)
        {
            if (container2D[tmpY][tmpX] == 0)
                // Flag slot as used
            {
                container2D[tmpY][tmpX] = 1;
            }
            else
            {
                throw new Exception($"Slot at({x}, {y}) is already filled. Cannot fit a {itemW} by {itemH} item");
            }
        }
    }
}

public class FindSlotResult
{
    public FindSlotResult(bool success)
    {
        Success = success;
    }

    public FindSlotResult(bool success, int x, int y, bool rotation)
    {
        Success = success;
        X = x;
        Y = y;
        Rotation = rotation;
    }

    public FindSlotResult()
    {
    }

    [JsonPropertyName("success")]
    public bool? Success
    {
        get;
        set;
    }

    [JsonPropertyName("x")]
    public int? X
    {
        get;
        set;
    }

    [JsonPropertyName("y")]
    public int? Y
    {
        get;
        set;
    }

    [JsonPropertyName("rotation")]
    public bool? Rotation
    {
        get;
        set;
    }
}
