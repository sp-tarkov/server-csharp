using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventorySplitRequestData : InventoryBaseActionRequestData
{
    /// <summary>
    ///     Id of item to split
    /// </summary>
    [JsonPropertyName("splitItem")]
    public string? SplitItem
    {
        get;
        set;
    }

    /// <summary>
    ///     Id of new item stack
    /// </summary>
    [JsonPropertyName("newItem")]
    public string? NewItem
    {
        get;
        set;
    }

    /// <summary>
    ///     Destination new item will be placed in
    /// </summary>
    [JsonPropertyName("container")]
    public Container? Container
    {
        get;
        set;
    }

    [JsonPropertyName("count")]
    public int? Count
    {
        get;
        set;
    }
}
