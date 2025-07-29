using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventorySplitRequestData : InventoryBaseActionRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    /// <summary>
    ///     Id of item to split
    /// </summary>
    [JsonPropertyName("splitItem")]
    public MongoId? SplitItem { get; set; }

    /// <summary>
    ///     Id of new item stack
    /// </summary>
    [JsonPropertyName("newItem")]
    public MongoId? NewItem { get; set; }

    /// <summary>
    ///     Destination new item will be placed in
    /// </summary>
    [JsonPropertyName("container")]
    public Container? Container { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}
