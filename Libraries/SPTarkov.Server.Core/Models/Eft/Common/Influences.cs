using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Influences
{
    [JsonPropertyName("LightBleeding")]
    public Influence? LightBleeding
    {
        get;
        set;
    }

    [JsonPropertyName("HeavyBleeding")]
    public Influence? HeavyBleeding
    {
        get;
        set;
    }

    [JsonPropertyName("Fracture")]
    public Influence? Fracture
    {
        get;
        set;
    }

    [JsonPropertyName("RadExposure")]
    public Influence? RadExposure
    {
        get;
        set;
    }

    [JsonPropertyName("Intoxication")]
    public Influence? Intoxication
    {
        get;
        set;
    }
}
