using System.Text.Json.Serialization;
using Core.Models.Enums;

namespace Core.Models.Eft.Common.Tables;

public record Reward
{
    [JsonPropertyName("value")]
    public object? Value { get; set; } // TODO: Can be either string or number

    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RewardType? Type { get; set; }

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("items")]
    public List<Item>? Items { get; set; }

    [JsonPropertyName("loyaltyLevel")]
    public int? LoyaltyLevel { get; set; }

    /** Hideout area id */
    [JsonPropertyName("traderId")]
    public object? TraderId { get; set; } // TODO: string | int

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded { get; set; }

    [JsonPropertyName("unknown")]
    public bool? Unknown { get; set; }

    [JsonPropertyName("findInRaid")]
    public bool? FindInRaid { get; set; }

    [JsonPropertyName("gameMode")]
    public List<string>? GameMode { get; set; }

    /** Game editions whitelisted to get reward */
    [JsonPropertyName("availableInGameEditions")]
    public List<string>? AvailableInGameEditions { get; set; }

    /** Game editions blacklisted from getting reward */
    [JsonPropertyName("notAvailableInGameEditions")]
    public List<string>? NotAvailableInGameEditions { get; set; }
    
    // This is always Null atm in the achievements.json
    [JsonPropertyName("illustrationConfig")]
    public object? IllustrationConfig { get; set; }
    
    [JsonPropertyName("isHidden")]
    public bool? IsHidden { get; set; }
}
