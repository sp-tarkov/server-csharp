using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ChancedEnemy
{
    [JsonPropertyName("EnemyChance")]
    public int? EnemyChance
    {
        get;
        set;
    }

    [JsonPropertyName("Role")]
    public string? Role
    {
        get;
        set;
    }
}
