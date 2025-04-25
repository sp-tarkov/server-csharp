using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BossLocationSpawn
{
    [JsonPropertyName("BossChance")]
    public double? BossChance
    {
        get;
        set;
    }

    [JsonPropertyName("BossDifficult")]
    public string? BossDifficulty
    {
        get;
        set;
    }

    [JsonPropertyName("BossEscortAmount")]
    public string? BossEscortAmount
    {
        get;
        set;
    }

    [JsonPropertyName("BossEscortDifficult")]
    public string? BossEscortDifficulty
    {
        get;
        set;
    }

    [JsonPropertyName("BossEscortType")]
    public string? BossEscortType
    {
        get;
        set;
    }

    [JsonPropertyName("BossName")]
    public string? BossName
    {
        get;
        set;
    }

    [JsonPropertyName("BossPlayer")]
    public bool? IsBossPlayer
    {
        get;
        set;
    }

    [JsonPropertyName("BossZone")]
    public string? BossZone
    {
        get;
        set;
    }

    [JsonPropertyName("RandomTimeSpawn")]
    public bool? IsRandomTimeSpawn
    {
        get;
        set;
    }

    [JsonPropertyName("Time")]
    public double? Time
    {
        get;
        set;
    }

    [JsonPropertyName("TriggerId")]
    public string? TriggerId
    {
        get;
        set;
    }

    [JsonPropertyName("TriggerName")]
    public string? TriggerName
    {
        get;
        set;
    }

    [JsonPropertyName("Delay")]
    public double? Delay
    {
        get;
        set;
    }

    [JsonPropertyName("DependKarma")]
    public bool? DependKarma
    {
        get;
        set;
    }

    [JsonPropertyName("DependKarmaPVE")]
    public bool? DependKarmaPVE
    {
        get;
        set;
    }

    [JsonPropertyName("ForceSpawn")]
    public bool? ForceSpawn
    {
        get;
        set;
    }

    [JsonPropertyName("IgnoreMaxBots")]
    public bool? IgnoreMaxBots
    {
        get;
        set;
    }


    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("Supports")]
    public List<BossSupport> Supports
    {
        get;
        set;
    }

    [JsonPropertyName("sptId")]
    public string? SptId
    {
        get;
        set;
    }

    [JsonPropertyName("SpawnMode")]
    public List<string> SpawnMode
    {
        get;
        set;
    }
}
