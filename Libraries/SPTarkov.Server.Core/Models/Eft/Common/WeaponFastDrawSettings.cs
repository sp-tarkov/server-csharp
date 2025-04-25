using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record WeaponFastDrawSettings
{
    [JsonPropertyName("HandShakeCurveFrequency")]
    public double? HandShakeCurveFrequency
    {
        get;
        set;
    }

    [JsonPropertyName("HandShakeCurveIntensity")]
    public double? HandShakeCurveIntensity
    {
        get;
        set;
    }

    [JsonPropertyName("HandShakeMaxDuration")]
    public double? HandShakeMaxDuration
    {
        get;
        set;
    }

    [JsonPropertyName("HandShakeTremorIntensity")]
    public double? HandShakeTremorIntensity
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponFastSwitchMaxSpeedMult")]
    public double? WeaponFastSwitchMaxSpeedMult
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponFastSwitchMinSpeedMult")]
    public double? WeaponFastSwitchMinSpeedMult
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponPistolFastSwitchMaxSpeedMult")]
    public double? WeaponPistolFastSwitchMaxSpeedMult
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponPistolFastSwitchMinSpeedMult")]
    public double? WeaponPistolFastSwitchMinSpeedMult
    {
        get;
        set;
    }
}
