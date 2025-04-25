using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record TradingSettings
{
    [JsonPropertyName("BuyRestrictionMaxBonus")]
    public Dictionary<string, BuyRestrictionMaxBonus>? BuyRestrictionMaxBonus
    {
        get;
        set;
    }

    [JsonPropertyName("BuyoutRestrictions")]
    public BuyoutRestrictions? BuyoutRestrictions
    {
        get;
        set;
    }
}
