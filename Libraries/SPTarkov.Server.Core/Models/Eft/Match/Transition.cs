using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record Transition
{
    [JsonPropertyName("transitionType")]
    public TransitionType? TransitionType
    {
        get;
        set;
    }

    [JsonPropertyName("transitionRaidId")]
    public string? TransitionRaidId
    {
        get;
        set;
    }

    [JsonPropertyName("transitionCount")]
    public int? TransitionCount
    {
        get;
        set;
    }

    [JsonPropertyName("visitedLocations")]
    public List<string>? VisitedLocations
    {
        get;
        set;
    }
}
