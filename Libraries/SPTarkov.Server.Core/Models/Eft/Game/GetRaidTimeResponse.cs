using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record GetRaidTimeResponse
{
    [JsonPropertyName("NewSurviveTimeSeconds")]
    public double? NewSurviveTimeSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("OriginalSurvivalTimeSeconds")]
    public double? OriginalSurvivalTimeSeconds
    {
        get;
        set;
    }
}
