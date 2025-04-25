using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record EffectDamageProps
{
    [JsonPropertyName("value")]
    public double? Value
    {
        get;
        set;
    }

    [JsonPropertyName("delay")]
    public double? Delay
    {
        get;
        set;
    }

    [JsonPropertyName("duration")]
    public double? Duration
    {
        get;
        set;
    }

    [JsonPropertyName("fadeOut")]
    public double? FadeOut
    {
        get;
        set;
    }

    [JsonPropertyName("cost")]
    public double? Cost
    {
        get;
        set;
    }

    [JsonPropertyName("healthPenaltyMin")]
    public double? HealthPenaltyMin
    {
        get;
        set;
    }

    [JsonPropertyName("healthPenaltyMax")]
    public double? HealthPenaltyMax
    {
        get;
        set;
    }
}
