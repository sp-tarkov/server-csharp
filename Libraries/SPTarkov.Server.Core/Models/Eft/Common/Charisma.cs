using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Charisma
{
    [JsonPropertyName("BonusSettings")]
    public BonusSettings? BonusSettings
    {
        get;
        set;
    }

    [JsonPropertyName("Counters")]
    public CharismaSkillCounters? Counters
    {
        get;
        set;
    }

    [JsonPropertyName("SkillProgressInt")]
    public double? SkillProgressInt
    {
        get;
        set;
    }

    [JsonPropertyName("SkillProgressAtn")]
    public double? SkillProgressAtn
    {
        get;
        set;
    }

    [JsonPropertyName("SkillProgressPer")]
    public double? SkillProgressPer
    {
        get;
        set;
    }
}
