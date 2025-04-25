using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Regeneration
{
    [JsonPropertyName("LoopTime")]
    public double? LoopTime
    {
        get;
        set;
    }

    [JsonPropertyName("MinimumHealthPercentage")]
    public double? MinimumHealthPercentage
    {
        get;
        set;
    }

    [JsonPropertyName("Energy")]
    public double? Energy
    {
        get;
        set;
    }

    [JsonPropertyName("Hydration")]
    public double? Hydration
    {
        get;
        set;
    }

    [JsonPropertyName("BodyHealth")]
    public BodyHealth? BodyHealth
    {
        get;
        set;
    }

    [JsonPropertyName("Influences")]
    public Influences? Influences
    {
        get;
        set;
    }
}
