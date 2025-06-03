using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record MatchConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind
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
    public required Dictionary<string, bool> RandomiseMapContainers
    {
        get;
        set;
    }
}
