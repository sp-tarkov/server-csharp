using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record SampleQuests
{
    [JsonPropertyName("_id")]
    public string? Id
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

    [JsonPropertyName("type")]
    public string? Type
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

    [JsonPropertyName("canShowNotificationsInGame")]
    public bool? CanShowNotificationsInGame
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

    [JsonPropertyName("conditions")]
    public QuestConditionTypes? Conditions
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

    [JsonPropertyName("description")]
    public string? Description
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

    [JsonPropertyName("failMessageText")]
    public string? FailMessageText
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

    [JsonPropertyName("templateId")]
    public string? TemplateId
    {
        get;
        set;
    }
}
