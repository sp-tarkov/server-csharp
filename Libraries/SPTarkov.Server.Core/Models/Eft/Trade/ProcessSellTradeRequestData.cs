using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Trade;

public record ProcessSellTradeRequestData : ProcessBaseTradeRequestData
{
    [JsonPropertyName("price")]
    public double? Price
    {
        get;
        set;
    }

    [JsonPropertyName("items")]
    public List<SoldItem>? Items
    {
        get;
        set;
    }
}
