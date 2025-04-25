using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Intellect
{
    public SkillRatio[] DependentSkillRatios
    {
        get;
        set;
    }

    [JsonPropertyName("Counters")]
    public IntellectCounters? Counters
    {
        get;
        set;
    }

    [JsonPropertyName("ExamineAction")]
    public double? ExamineAction
    {
        get;
        set;
    }

    [JsonPropertyName("SkillProgress")]
    public double? SkillProgress
    {
        get;
        set;
    }

    [JsonPropertyName("RepairAction")]
    public double? RepairAction
    {
        get;
        set;
    }

    [JsonPropertyName("WearAmountReducePerLevel")]
    public double? WearAmountReducePerLevel
    {
        get;
        set;
    }

    [JsonPropertyName("WearChanceReduceEliteLevel")]
    public double? WearChanceReduceEliteLevel
    {
        get;
        set;
    }

    [JsonPropertyName("RepairPointsCostReduction")]
    public double? RepairPointsCostReduction
    {
        get;
        set;
    }
}
