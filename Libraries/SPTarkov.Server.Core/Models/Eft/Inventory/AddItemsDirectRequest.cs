using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record AddItemsDirectRequest
{
    /// <summary>
    ///     Item and child mods to add to player inventory
    /// </summary>
    [JsonPropertyName("itemsWithModsToAdd")]
    public List<List<Item>>? ItemsWithModsToAdd
    {
        get;
        set;
    }

    [JsonPropertyName("foundInRaid")]
    public bool? FoundInRaid
    {
        get;
        set;
    }

    /// <summary>
    ///     Runs after EACH item with children is added
    /// </summary>
    [JsonPropertyName("callback")]
    public Action<int>? Callback
    {
        get;
        set;
    }

    /// <summary>
    ///     Should sorting table be used when no space found in stash
    /// </summary>
    [JsonPropertyName("useSortingTable")]
    public bool? UseSortingTable
    {
        get;
        set;
    }
}
