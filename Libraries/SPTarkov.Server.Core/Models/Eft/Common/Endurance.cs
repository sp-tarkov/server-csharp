using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Endurance
{
    [JsonPropertyName("MovementAction")]
    public double? MovementAction
    {
        get;
        set;
    }

    [JsonPropertyName("SprintAction")]
    public double? SprintAction
    {
        get;
        set;
    }

    [JsonPropertyName("GainPerFatigueStack")]
    public double? GainPerFatigueStack
    {
        get;
        set;
    }

    [JsonPropertyName("DependentSkillRatios")]
    public List<DependentSkillRatio>? DependentSkillRatios
    {
        get;
        set;
    }

    [JsonPropertyName("QTELevelMultipliers")]
    public Dictionary<string, Dictionary<string, double>>? QTELevelMultipliers
    {
        get;
        set;
    }
}
