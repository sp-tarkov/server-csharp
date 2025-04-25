using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record RepeatableQuest : Quest
{
    [JsonPropertyName("changeCost")]
    public List<ChangeCost?>? ChangeCost
    {
        get;
        set;
    }

    [JsonPropertyName("changeStandingCost")]
    public int? ChangeStandingCost
    {
        get;
        set;
    }

    [JsonPropertyName("sptRepatableGroupName")]
    public string? SptRepatableGroupName
    {
        get;
        set;
    }

    [JsonPropertyName("acceptanceAndFinishingSource")]
    public string? AcceptanceAndFinishingSource
    {
        get;
        set;
    }

    [JsonPropertyName("progressSource")]
    public string? ProgressSource
    {
        get;
        set;
    }

    [JsonPropertyName("rankingModes")]
    public List<string?>? RankingModes
    {
        get;
        set;
    }

    [JsonPropertyName("gameModes")]
    public List<string>? GameModes
    {
        get;
        set;
    }

    [JsonPropertyName("arenaLocations")]
    public List<string>? ArenaLocations
    {
        get;
        set;
    }

    [JsonPropertyName("questStatus")]
    public RepeatableQuestStatus? QuestStatus
    {
        get;
        set;
    }
}
