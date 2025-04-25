using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

/// <summary>
///     Same as BotBase.Quests
/// </summary>
public record QuestStatus
{
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("uid")]
    public string? Uid
    {
        get;
        set;
    }

    [JsonPropertyName("qid")]
    public string? QId
    {
        get;
        set;
    }

    [JsonPropertyName("startTime")]
    public double? StartTime
    {
        get;
        set;
    }

    [JsonPropertyName("status")]
    public QuestStatusEnum? Status
    {
        get;
        set;
    }

    [JsonPropertyName("statusTimers")]
    public Dictionary<QuestStatusEnum, double>? StatusTimers
    {
        get;
        set;
    }

    [JsonPropertyName("completedConditions")]
    public List<string>? CompletedConditions
    {
        get;
        set;
    }

    [JsonPropertyName("availableAfter")]
    public double? AvailableAfter
    {
        get;
        set;
    }
}
