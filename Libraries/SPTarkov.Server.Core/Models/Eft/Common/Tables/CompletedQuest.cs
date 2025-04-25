using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record CompletedQuest
{
    [JsonPropertyName("QuestId")]
    public string? QuestId
    {
        get;
        set;
    }
}
