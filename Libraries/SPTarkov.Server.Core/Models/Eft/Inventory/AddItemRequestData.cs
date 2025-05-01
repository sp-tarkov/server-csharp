using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record AddItemRequestData
{
    /// <summary>
    ///     Trader id
    /// </summary>
    [JsonPropertyName("tid")]
    public string? TraderId
    {
        get;
        set;
    }

    [JsonPropertyName("items")]
    public List<ItemToAdd>? Items
    {
        get;
        set;
    }
}

public record ItemToAdd
{
    [JsonPropertyName("count")]
    public int? Count
    {
        get;
        set;
    }

    [JsonPropertyName("sptIsPreset")]
    public bool? IsPreset
    {
        get;
        set;
    }

    [JsonPropertyName("item_id")]
    public string? ItemId
    {
        get;
        set;
    }
}
