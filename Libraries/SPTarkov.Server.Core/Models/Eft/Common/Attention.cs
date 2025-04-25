using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Attention
{
    [JsonPropertyName("DependentSkillRatios")]
    public SkillRatio[] DependentSkillRatios
    {
        get;
        set;
    }

    [JsonPropertyName("ExamineWithInstruction")]
    public double? ExamineWithInstruction
    {
        get;
        set;
    }

    [JsonPropertyName("FindActionFalse")]
    public double? FindActionFalse
    {
        get;
        set;
    }

    [JsonPropertyName("FindActionTrue")]
    public double? FindActionTrue
    {
        get;
        set;
    }
}
