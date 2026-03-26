using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record GetItemPricesResponse
{
    [JsonPropertyName("supplyNextTime")]
    public double? SupplyNextTime { get; set; }

    [JsonPropertyName("prices")]
    public Dictionary<MongoId, int>? Prices { get; set; }

    [JsonPropertyName("currencyCourses")]
    public Dictionary<string, int>? CurrencyCourses { get; set; }
}
