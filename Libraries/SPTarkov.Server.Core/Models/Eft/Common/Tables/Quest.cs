using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Json;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Quest
{
    /// <summary>
    ///     SPT addition - human readable quest name
    /// </summary>
    [JsonPropertyName("QuestName")]
    public string? QuestName
    {
        get;
        set;
    }

    /// <summary>
    ///     _id
    /// </summary>
    [JsonPropertyName("_id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("canShowNotificationsInGame")]
    public bool? CanShowNotificationsInGame
    {
        get;
        set;
    }

    [JsonPropertyName("conditions")]
    public QuestConditionTypes? Conditions
    {
        get;
        set;
    }

    [JsonPropertyName("description")]
    public string? Description
    {
        get;
        set;
    }

    [JsonPropertyName("failMessageText")]
    public string? FailMessageText
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

    [JsonPropertyName("note")]
    public string? Note
    {
        get;
        set;
    }

    [JsonPropertyName("traderId")]
    public string? TraderId
    {
        get;
        set;
    }

    [JsonPropertyName("location")]
    public string? Location
    {
        get;
        set;
    }

    [JsonPropertyName("image")]
    public string? Image
    {
        get;
        set;
    }

    [JsonPropertyName("type")] // can be string or QuestTypeEnum
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QuestTypeEnum? Type
    {
        get;
        set;
    }

    [JsonPropertyName("isKey")]
    public bool? IsKey
    {
        get;
        set;
    }

    [JsonPropertyName("restartable")]
    public bool? Restartable
    {
        get;
        set;
    }

    [JsonPropertyName("instantComplete")]
    public bool? InstantComplete
    {
        get;
        set;
    }

    [JsonPropertyName("secretQuest")]
    public bool? SecretQuest
    {
        get;
        set;
    }

    [JsonPropertyName("startedMessageText")]
    public string? StartedMessageText
    {
        get;
        set;
    }

    [JsonPropertyName("successMessageText")]
    public string? SuccessMessageText
    {
        get;
        set;
    }

    [JsonPropertyName("acceptPlayerMessage")]
    public string? AcceptPlayerMessage
    {
        get;
        set;
    }

    [JsonPropertyName("declinePlayerMessage")]
    public string? DeclinePlayerMessage
    {
        get;
        set;
    }

    [JsonPropertyName("completePlayerMessage")]
    public string? CompletePlayerMessage
    {
        get;
        set;
    }

    [JsonPropertyName("templateId")]
    public string? TemplateId
    {
        get;
        set;
    }

    [JsonPropertyName("rewards")]
    public QuestRewards? Rewards
    {
        get;
        set;
    }

    /// <summary>
    ///     Becomes 'AppearStatus' inside client
    /// </summary>
    [JsonPropertyName("status")]
    public object? Status
    {
        get;
        set;
    } // TODO: string | number

    [JsonPropertyName("KeyQuest")]
    public bool? KeyQuest
    {
        get;
        set;
    }

    [JsonPropertyName("changeQuestMessageText")]
    public string? ChangeQuestMessageText
    {
        get;
        set;
    }

    /// <summary>
    ///     "Pmc" or "Scav"
    /// </summary>
    [JsonPropertyName("side")]
    public string? Side
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
    public List<string>? RankingModes
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

    /// <summary>
    ///     Status of quest to player
    /// </summary>
    [JsonPropertyName("sptStatus")]
    public QuestStatusEnum? SptStatus
    {
        get;
        set;
    }

    [JsonPropertyName("questStatus")]
    public QuestStatus? QuestStatus
    {
        get;
        set;
    }

    [JsonPropertyName("changeCost")]
    public List<object> ChangeCost
    {
        get;
        set;
    }

    [JsonPropertyName("changeStandingCost")]
    public double ChangeStandingCost
    {
        get;
        set;
    }
}
