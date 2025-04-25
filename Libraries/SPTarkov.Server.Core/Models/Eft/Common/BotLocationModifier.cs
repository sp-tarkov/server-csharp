using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BotLocationModifier
{
    [JsonPropertyName("AccuracySpeed")]
    public double? AccuracySpeed
    {
        get;
        set;
    }

    [JsonPropertyName("AdditionalHostilitySettings")]
    public List<AdditionalHostilitySettings> AdditionalHostilitySettings
    {
        get;
        set;
    }

    [JsonPropertyName("DistToActivate")]
    public double? DistanceToActivate
    {
        get;
        set;
    }

    [JsonPropertyName("DistToActivatePvE")]
    public double? DistanceToActivatePvE
    {
        get;
        set;
    }

    [JsonPropertyName("DistToPersueAxemanCoef")]
    public double? DistanceToPursueAxemanCoefficient
    {
        get;
        set;
    }

    [JsonPropertyName("DistToSleep")]
    public double? DistanceToSleep
    {
        get;
        set;
    }

    [JsonPropertyName("DistToSleepPvE")]
    public double? DistanceToSleepPvE
    {
        get;
        set;
    }

    [JsonPropertyName("GainSight")]
    public double? GainSight
    {
        get;
        set;
    }

    [JsonPropertyName("KhorovodChance")]
    public double? KhorovodChance
    {
        get;
        set;
    }

    [JsonPropertyName("MagnetPower")]
    public double? MagnetPower
    {
        get;
        set;
    }

    [JsonPropertyName("MarksmanAccuratyCoef")]
    public double? MarksmanAccuracyCoefficient
    {
        get;
        set;
    }

    [JsonPropertyName("Scattering")]
    public double? Scattering
    {
        get;
        set;
    }

    [JsonPropertyName("VisibleDistance")]
    public double? VisibleDistance
    {
        get;
        set;
    }

    [JsonPropertyName("MaxExfiltrationTime")]
    public double? MaxExfiltrationTime
    {
        get;
        set;
    }

    [JsonPropertyName("MinExfiltrationTime")]
    public double? MinExfiltrationTime
    {
        get;
        set;
    }

    [JsonPropertyName("FogVisibilityDistanceCoef")]
    public double? FogVisibilityDistanceCoef
    {
        get;
        set;
    }

    [JsonPropertyName("FogVisibilitySpeedCoef")]
    public double? FogVisibilitySpeedCoef
    {
        get;
        set;
    }

    [JsonPropertyName("LockSpawnCheckRadius")]
    public double? FogVisibLockSpawnCheckRadiusilitySpeedCoef
    {
        get;
        set;
    }

    [JsonPropertyName("LockSpawnCheckRadiusPvE")]
    public double? LockSpawnCheckRadiusPvE
    {
        get;
        set;
    }

    [JsonPropertyName("LockSpawnStartTime")]
    public double? LockSpawnStartTime
    {
        get;
        set;
    }

    [JsonPropertyName("LockSpawnStartTimePvE")]
    public double? LockSpawnStartTimePvE
    {
        get;
        set;
    }

    [JsonPropertyName("LockSpawnStepTime")]
    public double? LockSpawnStepTime
    {
        get;
        set;
    }

    [JsonPropertyName("LockSpawnStepTimePvE")]
    public double? LockSpawnStepTimePvE
    {
        get;
        set;
    }

    [JsonPropertyName("NonWaveSpawnBotsLimitPerPlayer")]
    public double? NonWaveSpawnBotsLimitPerPlayer
    {
        get;
        set;
    }

    [JsonPropertyName("NonWaveSpawnBotsLimitPerPlayerPvE")]
    public double? NonWaveSpawnBotsLimitPerPlayerPvE
    {
        get;
        set;
    }

    [JsonPropertyName("RainVisibilityDistanceCoef")]
    public double? RainVisibilityDistanceCoef
    {
        get;
        set;
    }

    [JsonPropertyName("RainVisibilitySpeedCoef")]
    public double? RainVisibilitySpeedCoef
    {
        get;
        set;
    }
}
