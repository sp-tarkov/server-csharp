using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record RepeatableQuestStatus
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
    public long? StartTime
    {
        get;
        set;
    }

    [JsonPropertyName("status")]
    public int? Status
    {
        get;
        set;
    }

    [JsonPropertyName("statusTimers")]
    public object? StatusTimers
    {
        get;
        set;
    } // Use object for any type
}
