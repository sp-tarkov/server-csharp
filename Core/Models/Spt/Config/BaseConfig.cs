using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public class BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; }
}

public class RunIntervalValues
{
    [JsonPropertyName("inRaid")]
    public int InRaid { get; set; }

    [JsonPropertyName("outOfRaid")]
    public int OutOfRaid { get; set; }
}