using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Vitality
{
    [JsonPropertyName("DamageTakenAction")]
    public double? DamageTakenAction
    {
        get;
        set;
    }

    [JsonPropertyName("HealthNegativeEffect")]
    public double? HealthNegativeEffect
    {
        get;
        set;
    }
}
