using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public abstract record BaseConfig
{
    [JsonPropertyName("kind")]
    public abstract string Kind
    {
        get;
        set;
    }
}

public record RunIntervalValues
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("inRaid")]
    public int InRaid
    {
        get;
        set;
    }

    [JsonPropertyName("outOfRaid")]
    public int OutOfRaid
    {
        get;
        set;
    }
}
