using System.Text.Json.Serialization;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class ContainerHelper
{
    /// <summary>
    ///     Finds a slot for an item in a given 2D container map
    /// </summary>
    /// <param name="container2D">List of container with positions filled/free</param>
    /// <param name="itemX">Width of item</param>
    /// <param name="itemY">Height of item</param>
    /// <returns>Location to place item in container</returns>
    public FindSlotResult FindSlotForItem(int[][] container2D, int? itemX, int? itemY)
    {
        // Assume not rotated
        var rotation = false;

        var minVolume = (itemX < itemY ? itemX : itemY) - 1;
        var containerY = container2D.Length;
        var containerX = container2D[0].Length;
        var limitY = containerY - minVolume;
        var limitX = containerX - minVolume;

        // Every x+y slot taken up in container, exit
        if (
            container2D.All(x =>
            {
                return x.All(y =>
                {
                    return y == 1;
                });
            })
        )
        {
            return new FindSlotResult(false);
        }

        // Down = y
        for (var y = 0; y < limitY; y++)
        {
            if (
                container2D[y]
                    .All(x =>
                    {
                        return x == 1;
                    })
            )
            // Every item in row is full, skip row
            {
                continue;
            }

            // Go left to right across x-axis looking for free position
            for (var x = 0; x < limitX; x++)
            {
                if (
                    CanItemBePlacedInContainerAtPosition(
                        container2D,
                        containerX,
                        containerY,
                        x,
                        y,
                        itemX!.Value,
                        itemY!.Value
                    )
                )
                {
                    // Success, return result
                    return new FindSlotResult(true, x, y, rotation);
                }

                if (ItemBiggerThan1X1(itemX!.Value, itemY!.Value))
                {
                    // Pointless rotating a 1x1, try next position across
                    continue;
                }

                // Bigger than 1x1, try rotating by swapping x and y values
                if (
                    !CanItemBePlacedInContainerAtPosition(
                        container2D,
                        containerX,
                        containerY,
                        x,
                        y,
                        itemY!.Value,
                        itemX!.Value
                    )
                )
                {
                    continue;
                }

                // Found a position for item when rotated
                rotation = true;

                return new FindSlotResult(true, x, y, rotation);
            }
        }

        // Tried all possible positions, nothing big enough for item
        return new FindSlotResult(false);
    }

    protected static bool ItemBiggerThan1X1(int itemWidth, int itemHeight)
    {
        return itemWidth + itemHeight > 2;
    }

    /// <summary>
    ///     Can an item of specified size be placed inside a 2d container at a specific position
    /// </summary>
    /// <param name="container">Container to find space in</param>
    /// <param name="containerWidth">Container x size</param>
    /// <param name="containerHeight">Container y size</param>
    /// <param name="startXPos">Starting x position for item</param>
    /// <param name="startYPos">Starting y position for item</param>
    /// <param name="itemWidth">Items width</param>
    /// <param name="itemHeight">Items height</param>
    /// <returns>True - slot found</returns>
    protected bool CanItemBePlacedInContainerAtPosition(
        int[][] container,
        int containerWidth,
        int containerHeight,
        int startXPos,
        int startYPos,
        int itemWidth,
        int itemHeight
    )
    {
        // Check item isn't bigger than container when at position
        if (startXPos + itemWidth > containerWidth || startYPos + itemHeight > containerHeight)
        {
            return false;
        }

        // Check each position item will take up in container, go across and then down
        for (var itemY = startYPos; itemY < startYPos + itemHeight; itemY++)
        {
            for (var itemX = startXPos; itemX < startXPos + itemWidth; itemX++)
            {
                // e,g for a 2x2 item; [0,0] then [0,1] then [1,0] then [1,1]
                if (container[itemY][itemX] != 0)
                {
                    // x,y Position blocked, can't place
                    return false;
                }
            }
        }

        return true;
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
        int? itemW,
        int? itemH,
        bool rotate
    )
    {
        // Swap height/width if we want to fit it in rotated
        var itemWidth = rotate ? itemH : itemW;
        var itemHeight = rotate ? itemW : itemH;

        for (var tmpY = y; tmpY < y + itemHeight; tmpY++)
        {
            for (var tmpX = x; tmpX < x + itemWidth; tmpX++)
            {
                if (container2D[tmpY][tmpX] == 0)
                // Flag slot as used
                {
                    container2D[tmpY][tmpX] = 1;
                }
                else
                {
                    throw new Exception(
                        $"Slot at({x}, {y}) is already filled. Cannot fit a {itemW} by {itemH} item"
                    );
                }
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

    public FindSlotResult() { }

    [JsonPropertyName("success")]
    public bool? Success { get; set; }

    [JsonPropertyName("x")]
    public int? X { get; set; }

    [JsonPropertyName("y")]
    public int? Y { get; set; }

    [JsonPropertyName("rotation")]
    public bool? Rotation { get; set; }
}
