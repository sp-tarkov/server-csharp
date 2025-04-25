using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record PriceModifier
{
    [JsonPropertyName("PriceModifier")]
    public double? PriceModifierValue
    {
        get;
        set;
    }
}
