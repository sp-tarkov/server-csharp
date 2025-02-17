using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

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
