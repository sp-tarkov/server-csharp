using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public record GetItemPricesResponse
{
    [JsonPropertyName("supplyNextTime")]
    public double? SupplyNextTime
    {
        get;
        set;
    }

    [JsonPropertyName("prices")]
    public Dictionary<string, double>? Prices
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
