using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Exp
{
    [JsonPropertyName("heal")]
    public Heal? Heal
    {
        get;
        set;
    }

    [JsonPropertyName("match_end")]
    public MatchEnd? MatchEnd
    {
        get;
        set;
    }

    [JsonPropertyName("kill")]
    public Kill? Kill
    {
        get;
        set;
    }

    [JsonPropertyName("level")]
    public Level? Level
    {
        get;
        set;
    }

    [JsonPropertyName("loot_attempts")]
    public List<LootAttempt>? LootAttempts
    {
        get;
        set;
    }

    // Confirmed in client
    [JsonPropertyName("expForLevelOneDogtag")]
    public double? ExpForLevelOneDogtag
    {
        get;
        set;
    }

    // Confirmed in client
    [JsonPropertyName("expForLockedDoorOpen")]
    public int? ExpForLockedDoorOpen
    {
        get;
        set;
    }

    // Confirmed in client
    [JsonPropertyName("expForLockedDoorBreach")]
    public int? ExpForLockedDoorBreach
    {
        get;
        set;
    }

    [JsonPropertyName("triggerMult")]
    public double? TriggerMult
    {
        get;
        set;
    }
}
