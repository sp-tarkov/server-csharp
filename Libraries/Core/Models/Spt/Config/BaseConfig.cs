using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public record BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; }
}

public record RunIntervalValues
{
    [JsonPropertyName("inRaid")]
    public int InRaid { get; set; }

    [JsonPropertyName("outOfRaid")]
    public int OutOfRaid { get; set; }
}
