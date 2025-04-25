using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Halloween2024
{
    [JsonPropertyName("CrowdAttackBlockRadius")]
    public double? CrowdAttackBlockRadius
    {
        get;
        set;
    }

    [JsonPropertyName("CrowdAttackSpawnParams")]
    public List<CrowdAttackSpawnParam>? CrowdAttackSpawnParams
    {
        get;
        set;
    }

    [JsonPropertyName("CrowdCooldownPerPlayerSec")]
    public double? CrowdCooldownPerPlayerSec
    {
        get;
        set;
    }

    [JsonPropertyName("CrowdsLimit")]
    public int? CrowdsLimit
    {
        get;
        set;
    }

    [JsonPropertyName("InfectedLookCoeff")]
    public double? InfectedLookCoeff
    {
        get;
        set;
    }

    [JsonPropertyName("MaxCrowdAttackSpawnLimit")]
    public int? MaxCrowdAttackSpawnLimit
    {
        get;
        set;
    }

    [JsonPropertyName("MinInfectionPercentage")]
    public double? MinInfectionPercentage
    {
        get;
        set;
    }

    [JsonPropertyName("MinSpawnDistToPlayer")]
    public double? MinSpawnDistToPlayer
    {
        get;
        set;
    }

    [JsonPropertyName("TargetPointSearchRadiusLimit")]
    public double? TargetPointSearchRadiusLimit
    {
        get;
        set;
    }

    [JsonPropertyName("ZombieCallDeltaRadius")]
    public double? ZombieCallDeltaRadius
    {
        get;
        set;
    }

    [JsonPropertyName("ZombieCallPeriodSec")]
    public double? ZombieCallPeriodSec
    {
        get;
        set;
    }

    [JsonPropertyName("ZombieCallRadiusLimit")]
    public double? ZombieCallRadiusLimit
    {
        get;
        set;
    }

    [JsonPropertyName("ZombieMultiplier")]
    public double? ZombieMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("InfectionPercentage")]
    public double? InfectionPercentage
    {
        get;
        set;
    }

    public Khorovod? Khorovod
    {
        get;
        set;
    }
}
