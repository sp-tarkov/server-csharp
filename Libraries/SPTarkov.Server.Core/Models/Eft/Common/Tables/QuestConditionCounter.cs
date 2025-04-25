using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record QuestConditionCounter
{
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("conditions")]
    public List<QuestConditionCounterCondition>? Conditions
    {
        get;
        set;
    }
}
