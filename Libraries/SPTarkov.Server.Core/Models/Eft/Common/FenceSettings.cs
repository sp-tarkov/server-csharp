using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record FenceSettings
{
    // MongoId
    [JsonPropertyName("FenceId")]
    public string? FenceIdentifier
    {
        get;
        set;
    }

    [JsonPropertyName("Levels")]
    public Dictionary<double, FenceLevel>? Levels
    {
        get;
        set;
    }

    [JsonPropertyName("paidExitStandingNumerator")]
    public double? PaidExitStandingNumerator
    {
        get;
        set;
    }

    public double? PmcBotKillStandingMultiplier
    {
        get;
        set;
    }
}
