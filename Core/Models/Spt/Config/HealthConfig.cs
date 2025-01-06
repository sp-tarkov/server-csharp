using System.Text.Json.Serialization;

namespace Types.Models.Spt.Config;

public class HealthConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-health";

    [JsonPropertyName("healthMultipliers")]
    public HealthMultipliers HealthMultipliers { get; set; }

    [JsonPropertyName("save")]
    public Save Save { get; set; }
}

public class HealthMultipliers
{
    [JsonPropertyName("death")]
    public double Death { get; set; }

    [JsonPropertyName("blacked")]
    public double Blacked { get; set; }
}

public class Save
{
    [JsonPropertyName("health")]
    public bool Health { get; set; }

    [JsonPropertyName("effects")]
    public bool Effects { get; set; }
}