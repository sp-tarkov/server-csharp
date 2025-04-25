using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record AdditionalHostilitySettings
{
    [JsonPropertyName("AlwaysEnemies")]
    public List<string>? AlwaysEnemies
    {
        get;
        set;
    }

    [JsonPropertyName("AlwaysFriends")]
    public List<string>? AlwaysFriends
    {
        get;
        set;
    }

    [JsonPropertyName("BearEnemyChance")]
    public double? BearEnemyChance
    {
        get;
        set;
    }

    [JsonPropertyName("BearPlayerBehaviour")]
    public string? BearPlayerBehaviour
    {
        get;
        set;
    }

    [JsonPropertyName("BotRole")]
    public string? BotRole
    {
        get;
        set;
    }

    [JsonPropertyName("ChancedEnemies")]
    public List<ChancedEnemy>? ChancedEnemies
    {
        get;
        set;
    }

    [JsonPropertyName("Neutral")]
    public List<string>? Neutral
    {
        get;
        set;
    }

    [JsonPropertyName("SavagePlayerBehaviour")]
    public string? SavagePlayerBehaviour
    {
        get;
        set;
    }

    [JsonPropertyName("SavageEnemyChance")]
    public double? SavageEnemyChance
    {
        get;
        set;
    }

    [JsonPropertyName("UsecEnemyChance")]
    public double? UsecEnemyChance
    {
        get;
        set;
    }

    [JsonPropertyName("UsecPlayerBehaviour")]
    public string? UsecPlayerBehaviour
    {
        get;
        set;
    }

    [JsonPropertyName("Warn")]
    public List<string>? Warn
    {
        get;
        set;
    }
}
