using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Eft.Inventory;

public record AddItemDirectRequest
{
    /// <summary>
    /// Item and child mods to add to player inventory
    /// </summary>
    [JsonPropertyName("itemWithModsToAdd")]
    public List<Item>? ItemWithModsToAdd { get; set; }

    [JsonPropertyName("foundInRaid")]
    public bool? FoundInRaid { get; set; }

    [JsonPropertyName("callback")]
    public Action<int>? Callback { get; set; }

    [JsonPropertyName("useSortingTable")]
    public bool? UseSortingTable { get; set; }
}
