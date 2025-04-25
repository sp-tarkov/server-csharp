using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record CharismaSkillCounters
{
    [JsonPropertyName("insuranceCost")]
    public SkillCounter? InsuranceCost
    {
        get;
        set;
    }

    [JsonPropertyName("repairCost")]
    public SkillCounter? RepairCost
    {
        get;
        set;
    }

    [JsonPropertyName("repeatableQuestCompleteCount")]
    public SkillCounter? RepeatableQuestCompleteCount
    {
        get;
        set;
    }

    [JsonPropertyName("restoredHealthCost")]
    public SkillCounter? RestoredHealthCost
    {
        get;
        set;
    }

    [JsonPropertyName("scavCaseCost")]
    public SkillCounter? ScavCaseCost
    {
        get;
        set;
    }
}
