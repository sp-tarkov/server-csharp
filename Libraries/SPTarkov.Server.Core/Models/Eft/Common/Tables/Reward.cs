using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Reward
{
    [JsonPropertyName("value")]
    public object? Value
    {
        get;
        set;
    } // TODO: Can be either string or number

    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RewardType? Type
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

    [JsonPropertyName("target")]
    public string? Target
    {
        get;
        set;
    }

    [JsonPropertyName("items")]
    public List<Item>? Items
    {
        get;
        set;
    }

    [JsonPropertyName("loyaltyLevel")]
    public int? LoyaltyLevel
    {
        get;
        set;
    }

    /// <summary>
    /// Hideout area id
    /// </summary>
    [JsonPropertyName("traderId")]
    public object? TraderId
    {
        get;
        set;
    } // TODO: string | int

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded
    {
        get;
        set;
    }

    [JsonPropertyName("unknown")]
    public bool? Unknown
    {
        get;
        set;
    }

    [JsonPropertyName("findInRaid")]
    public bool? FindInRaid
    {
        get;
        set;
    }

    [JsonPropertyName("gameMode")]
    public List<string>? GameMode
    {
        get;
        set;
    }

    /// <summary>
    /// Game editions whitelisted to get reward
    /// </summary>
    [JsonPropertyName("availableInGameEditions")]
    public HashSet<string>? AvailableInGameEditions
    {
        get;
        set;
    }

    /// <summary>
    /// Game editions blacklisted from getting reward
    /// </summary>
    [JsonPropertyName("notAvailableInGameEditions")]
    public HashSet<string>? NotAvailableInGameEditions
    {
        get;
        set;
    }

    // This is always Null atm in the achievements.json
    [JsonPropertyName("illustrationConfig")]
    public object? IllustrationConfig
    {
        get;
        set;
    }

    [JsonPropertyName("isHidden")]
    public bool? IsHidden
    {
        get;
        set;
    }
}
