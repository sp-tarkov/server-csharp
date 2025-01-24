using System.Text.Json.Serialization;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Trade;

namespace Core.Models.Eft.Inventory;

public record AddItemsDirectRequest
{
    /// Item and child mods to add to player inventory
    [JsonPropertyName("itemsWithModsToAdd")]
    public List<List<Item>>? ItemsWithModsToAdd { get; set; }

    [JsonPropertyName("foundInRaid")]
    public bool? FoundInRaid { get; set; }

    /// Runs after EACH item with children is added
    [JsonPropertyName("callback")]
    public Action<int>? Callback { get; set; }

    /// Should sorting table be used when no space found in stash
    [JsonPropertyName("useSortingTable")]
    public bool? UseSortingTable { get; set; }
}
