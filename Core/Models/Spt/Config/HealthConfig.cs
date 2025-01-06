using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public class HealthConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-health";

    [JsonPropertyName("healthMultipliers")]
    public HealthMultipliers HealthMultipliers { get; set; }

    [JsonPropertyName("save")]
    public HealthSave Save { get; set; }
}

public class HealthMultipliers
{
    [JsonPropertyName("death")]
    public double Death { get; set; }

    [JsonPropertyName("blacked")]
    public double Blacked { get; set; }
}

public class HealthSave
{
    [JsonPropertyName("health")]
    public bool Health { get; set; }

    [JsonPropertyName("effects")]
    public bool Effects { get; set; }
}