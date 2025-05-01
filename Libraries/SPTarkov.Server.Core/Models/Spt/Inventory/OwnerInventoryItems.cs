using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Inventory;

public record OwnerInventoryItems
{
    /// <summary>
    ///     Inventory items from source
    /// </summary>
    [JsonPropertyName("from")]
    public List<Item>? From
    {
        get;
        set;
    }

    /// <summary>
    ///     Inventory items at destination
    /// </summary>
    [JsonPropertyName("to")]
    public List<Item>? To
    {
        get;
        set;
    }

    [JsonPropertyName("sameInventory")]
    public bool? SameInventory
    {
        get;
        set;
    }

    [JsonPropertyName("isMail")]
    public bool? IsMail
    {
        get;
        set;
    }
}
