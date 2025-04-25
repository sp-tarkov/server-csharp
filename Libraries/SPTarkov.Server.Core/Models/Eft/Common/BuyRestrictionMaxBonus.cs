using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BuyRestrictionMaxBonus
{
    [JsonPropertyName("multiplier")]
    public double? Multiplier
    {
        get;
        set;
    }
}
