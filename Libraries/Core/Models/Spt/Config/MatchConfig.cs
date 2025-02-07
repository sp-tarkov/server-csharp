using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public record MatchConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind
    {
        get;
        set;
    } = "spt-match";

    [JsonPropertyName("enabled")]
    public bool Enabled
    {
        get;
        set;
    }

    [JsonPropertyName("randomiseMapContainers")]
    public Dictionary<string, bool> RandomiseMapContainers
    {
        get;
        set;
    }
}
