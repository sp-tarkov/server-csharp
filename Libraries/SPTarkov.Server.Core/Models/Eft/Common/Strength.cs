using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Strength
{
    [JsonPropertyName("DependentSkillRatios")]
    public List<DependentSkillRatio>? DependentSkillRatios
    {
        get;
        set;
    }

    [JsonPropertyName("SprintActionMin")]
    public double? SprintActionMin
    {
        get;
        set;
    }

    [JsonPropertyName("SprintActionMax")]
    public double? SprintActionMax
    {
        get;
        set;
    }

    [JsonPropertyName("MovementActionMin")]
    public double? MovementActionMin
    {
        get;
        set;
    }

    [JsonPropertyName("MovementActionMax")]
    public double? MovementActionMax
    {
        get;
        set;
    }

    [JsonPropertyName("PushUpMin")]
    public double? PushUpMin
    {
        get;
        set;
    }

    [JsonPropertyName("PushUpMax")]
    public double? PushUpMax
    {
        get;
        set;
    }

    [JsonPropertyName("QTELevelMultipliers")]
    public List<QTELevelMultiplier>? QTELevelMultipliers
    {
        get;
        set;
    }

    [JsonPropertyName("FistfightAction")]
    public double? FistfightAction
    {
        get;
        set;
    }

    [JsonPropertyName("ThrowAction")]
    public double? ThrowAction
    {
        get;
        set;
    }
}
