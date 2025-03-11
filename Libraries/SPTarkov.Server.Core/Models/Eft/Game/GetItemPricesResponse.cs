using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record GetItemPricesResponse
{
    [JsonPropertyName("supplyNextTime")]
    public double? SupplyNextTime
    {
        get;
        set;
    }

    [JsonPropertyName("prices")]
    public Dictionary<MongoId, double>? Prices
    {
        get;
        set;
    }

    [JsonPropertyName("currencyCourses")]
    public Dictionary<string, double>? CurrencyCourses
    {
        get;
        set;
    }
}
