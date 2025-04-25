using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Immunity
{
    [JsonPropertyName("ImmunityMiscEffects")]
    public double? ImmunityMiscEffects
    {
        get;
        set;
    }

    [JsonPropertyName("ImmunityPoisonBuff")]
    public double? ImmunityPoisonBuff
    {
        get;
        set;
    }

    [JsonPropertyName("ImmunityPainKiller")]
    public double? ImmunityPainKiller
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

    [JsonPropertyName("StimulatorNegativeBuff")]
    public double? StimulatorNegativeBuff
    {
        get;
        set;
    }
}
