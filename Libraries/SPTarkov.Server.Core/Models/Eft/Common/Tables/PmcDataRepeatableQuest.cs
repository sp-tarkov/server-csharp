using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record PmcDataRepeatableQuest
{
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonPropertyName("unavailableTime")]
    public string? UnavailableTime
    {
        get;
        set;
    }

    [JsonPropertyName("activeQuests")]
    public List<RepeatableQuest>? ActiveQuests
    {
        get;
        set;
    }

    [JsonPropertyName("inactiveQuests")]
    public List<RepeatableQuest>? InactiveQuests
    {
        get;
        set;
    }

    [JsonPropertyName("endTime")]
    public long? EndTime
    {
        get;
        set;
    }

    /// <summary>
    ///     What it costs to reset: QuestId, ChangeRequirement. Redundant to change requirements within RepeatableQuest
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("changeRequirement")]
    public Dictionary<string?, ChangeRequirement?>? ChangeRequirement
    {
        get;
        set;
    }

    [JsonPropertyName("freeChanges")]
    public int? FreeChanges
    {
        get;
        set;
    }

    [JsonPropertyName("freeChangesAvailable")]
    public int? FreeChangesAvailable
    {
        get;
        set;
    }
}
