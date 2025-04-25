using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record QuestSettings
{
    [JsonPropertyName("GlobalRewardRepModifierDailyQuestPvE")]
    public double? GlobalRewardRepModifierDailyQuestPvE
    {
        get;
        set;
    }

    [JsonPropertyName("GlobalRewardRepModifierQuestPvE")]
    public double? GlobalRewardRepModifierQuestPvE
    {
        get;
        set;
    }
}
