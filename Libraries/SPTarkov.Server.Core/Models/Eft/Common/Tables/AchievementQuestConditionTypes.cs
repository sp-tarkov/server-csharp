using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record AchievementQuestConditionTypes
{
    [JsonPropertyName("started")]
    public List<QuestCondition>? Started
    {
        get;
        set;
    }

    [JsonPropertyName("availableForFinish")]
    public List<QuestCondition>? AvailableForFinish
    {
        get;
        set;
    }

    [JsonPropertyName("availableForStart")]
    public List<QuestCondition>? AvailableForStart
    {
        get;
        set;
    }

    [JsonPropertyName("success")]
    public List<QuestCondition>? Success
    {
        get;
        set;
    }

    [JsonPropertyName("fail")]
    public List<QuestCondition>? Fail
    {
        get;
        set;
    }
}
