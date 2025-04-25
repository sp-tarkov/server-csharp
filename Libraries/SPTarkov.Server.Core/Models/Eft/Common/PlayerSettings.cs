using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record PlayerSettings
{
    [JsonPropertyName("BaseMaxMovementRolloff")]
    public double? BaseMaxMovementRolloff
    {
        get;
        set;
    }

    [JsonPropertyName("EnabledOcclusionDynamicRolloff")]
    public bool? IsEnabledOcclusionDynamicRolloff
    {
        get;
        set;
    }

    [JsonPropertyName("IndoorRolloffMult")]
    public double? IndoorRolloffMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("MinStepSoundRolloffMult")]
    public double? MinStepSoundRolloffMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("MinStepSoundVolumeMult")]
    public double? MinStepSoundVolumeMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("MovementRolloffMultipliers")]
    public List<MovementRolloffMultiplier>? MovementRolloffMultipliers
    {
        get;
        set;
    }

    [JsonPropertyName("OutdoorRolloffMult")]
    public double? OutdoorRolloffMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("SearchSoundVolume")]
    public SearchSoundVolumeSettings? SearchSoundVolume
    {
        get;
        set;
    }
}
