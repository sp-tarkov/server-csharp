using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Trade;

public record ProcessBuyTradeRequestData : ProcessBaseTradeRequestData
{
    [JsonPropertyName("item_id")]
    public string? ItemId
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

    [JsonPropertyName("scheme_id")]
    public int? SchemeId
    {
        get;
        set;
    }

    /// <summary>
    /// Id of stack to take money from, is money tpl when Action is `SptInsure`
    /// </summary>
    [JsonPropertyName("scheme_items")]
    public List<IdWithCount>? SchemeItems
    {
        get;
        set;
    }
}
