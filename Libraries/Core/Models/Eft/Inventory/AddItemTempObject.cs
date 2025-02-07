using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Eft.Inventory;

public record AddItemTempObject
{
    [JsonPropertyName("itemRef")]
    public Item? ItemReference
    {
        get;
        set;
    }

    [JsonPropertyName("count")]
    public int? ItemCount
    {
        get;
        set;
    }

    [JsonPropertyName("isPreset")]
    public bool? IsPresetItem
    {
        get;
        set;
    }

    [JsonPropertyName("location")]
    public ItemLocation? ItemLocation
    {
        get;
        set;
    }

    // Container item will be placed in - stash or sorting table
    [JsonPropertyName("containerId")]
    public string? ContainerIdentifier
    {
        get;
        set;
    }
}
