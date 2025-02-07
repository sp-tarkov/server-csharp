using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Spt.Quests;

public record GetRepeatableByIdResult
{
    [JsonPropertyName("quest")]
    public RepeatableQuest? Quest
    {
        get;
        set;
    }

    [JsonPropertyName("repeatableType")]
    public PmcDataRepeatableQuest? RepeatableType
    {
        get;
        set;
    }
}
