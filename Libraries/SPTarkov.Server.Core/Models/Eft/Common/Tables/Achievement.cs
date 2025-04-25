using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Achievement
{
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl
    {
        get;
        set;
    }

    [JsonPropertyName("assetPath")]
    public string? AssetPath
    {
        get;
        set;
    }

    [JsonPropertyName("rewards")]
    public List<Reward>? Rewards
    {
        get;
        set;
    }

    [JsonPropertyName("conditions")]
    public AchievementQuestConditionTypes? Conditions
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

    [JsonPropertyName("showNotificationsInGame")]
    public bool? ShowNotificationsInGame
    {
        get;
        set;
    }

    [JsonPropertyName("showProgress")]
    public bool? ShowProgress
    {
        get;
        set;
    }

    [JsonPropertyName("prefab")]
    public string? Prefab
    {
        get;
        set;
    }

    [JsonPropertyName("rarity")]
    public string? Rarity
    {
        get;
        set;
    }

    [JsonPropertyName("hidden")]
    public bool? Hidden
    {
        get;
        set;
    }

    [JsonPropertyName("showConditions")]
    public bool? ShowConditions
    {
        get;
        set;
    }

    [JsonPropertyName("progressBarEnabled")]
    public bool? ProgressBarEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("side")]
    public string? Side
    {
        get;
        set;
    }

    [JsonPropertyName("index")]
    public int? Index
    {
        get;
        set;
    }
}
