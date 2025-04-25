using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BuyoutRestrictions
{
    [JsonPropertyName("MinDurability")]
    public double? MinDurability
    {
        get;
        set;
    }

    [JsonPropertyName("MinFoodDrinkResource")]
    public double? MinFoodDrinkResource
    {
        get;
        set;
    }

    [JsonPropertyName("MinMedsResource")]
    public double? MinMedsResource
    {
        get;
        set;
    }
}
