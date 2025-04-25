using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Buff
{
    [JsonPropertyName("BuffType")]
    public string? BuffType
    {
        get;
        set;
    }

    [JsonPropertyName("Chance")]
    public double? Chance
    {
        get;
        set;
    }

    [JsonPropertyName("Delay")]
    public double? Delay
    {
        get;
        set;
    }

    [JsonPropertyName("Duration")]
    public double? Duration
    {
        get;
        set;
    }

    [JsonPropertyName("Value")]
    public double? Value
    {
        get;
        set;
    }

    [JsonPropertyName("AbsoluteValue")]
    public bool? AbsoluteValue
    {
        get;
        set;
    }

    [JsonPropertyName("SkillName")]
    public string? SkillName
    {
        get;
        set;
    }

    public List<string>? AppliesTo
    {
        get;
        set;
    }
}
