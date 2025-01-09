using System.Text.Json.Serialization;

namespace Core.Helpers;

public class ContainerHelper
{
    /// <summary>
    /// Finds a slot for an item in a given 2D container map
    /// </summary>
    /// <param name="container2D">List of container with slots filled/free</param>
    /// <param name="itemWidth">Width of item</param>
    /// <param name="itemHeight">Height of item</param>
    /// <returns>Location to place item in container</returns>
    public FindSlotResult FindSlotForItem(List<List<int>> container2D, int itemWidth, int itemHeight)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find a slot inside a container an item can be placed in
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
        List<List<int>> container2D,
        int containerX,
        int containerY,
        int x,
        int y,
        int itemW,
        int itemH)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find a free slot for an item to be placed at
    /// </summary>
    /// <param name="container2D">Container to place item in</param>
    /// <param name="x">Container x size</param>
    /// <param name="y">Container y size</param>
    /// <param name="itemW">Items width</param>
    /// <param name="itemH">Items height</param>
    /// <param name="rotate">is item rotated</param>
    public void FillContainerMapWithItem(
        List<List<int>> container2D,
        int x,
        int y,
        int itemW,
        int itemH,
        bool rotate)
    {
        throw new NotImplementedException();
    }
}

public class FindSlotResult
{
    [JsonPropertyName("success")]
    public bool? Success { get; set; }
    
    [JsonPropertyName("x")]
    public double? X { get; set; }
    
    [JsonPropertyName("y")]
    public double? Y { get; set; }
    
    [JsonPropertyName("rotation")]
    public bool? Rotation { get; set; }
}
