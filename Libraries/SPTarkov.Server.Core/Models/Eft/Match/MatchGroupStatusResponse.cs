using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record MatchGroupStatusResponse
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("players")]
    public List<GroupCharacter>? Players
    {
        get;
        set;
    }

    [JsonPropertyName("maxPveCountExceeded")]
    public bool? MaxPveCountExceeded
    {
        get;
        set;
    }
}
