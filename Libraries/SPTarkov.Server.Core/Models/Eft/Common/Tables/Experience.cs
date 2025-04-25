using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Experience
{
    /// <summary>
    ///     key = bot difficulty
    /// </summary>
    [JsonPropertyName("aggressorBonus")]
    public Dictionary<string, double>? AggressorBonus
    {
        get;
        set;
    }

    [JsonPropertyName("level")]
    public MinMax<int>? Level
    {
        get;
        set;
    }

    /// <summary>
    ///     key = bot difficulty
    /// </summary>
    [JsonPropertyName("reward")]
    public Dictionary<string, MinMax<int>>? Reward
    {
        get;
        set;
    }

    /// <summary>
    ///     key = bot difficulty
    /// </summary>
    [JsonPropertyName("standingForKill")]
    public Dictionary<string, double>? StandingForKill
    {
        get;
        set;
    }

    [JsonPropertyName("useSimpleAnimator")]
    public bool? UseSimpleAnimator
    {
        get;
        set;
    }
}
