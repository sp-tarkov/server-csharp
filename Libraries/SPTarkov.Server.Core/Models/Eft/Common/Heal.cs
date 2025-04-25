using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Heal
{
    [JsonPropertyName("expForHeal")]
    public double? ExpForHeal
    {
        get;
        set;
    }

    [JsonPropertyName("expForHydration")]
    public double? ExpForHydration
    {
        get;
        set;
    }

    [JsonPropertyName("expForEnergy")]
    public double? ExpForEnergy
    {
        get;
        set;
    }
}
