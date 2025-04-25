using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record QuestConditionTypes
{
    [JsonPropertyName("Started")]
    public List<QuestCondition>? Started
    {
        get;
        set;
    }

    [JsonPropertyName("AvailableForFinish")]
    public List<QuestCondition>? AvailableForFinish
    {
        get;
        set;
    }

    [JsonPropertyName("AvailableForStart")]
    public List<QuestCondition>? AvailableForStart
    {
        get;
        set;
    }

    [JsonPropertyName("Success")]
    public List<QuestCondition>? Success
    {
        get;
        set;
    }

    [JsonPropertyName("Fail")]
    public List<QuestCondition>? Fail
    {
        get;
        set;
    }
}
