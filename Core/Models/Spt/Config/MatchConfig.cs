using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public class MatchConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-match";

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}