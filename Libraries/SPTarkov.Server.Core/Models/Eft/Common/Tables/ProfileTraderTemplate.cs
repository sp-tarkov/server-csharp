using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record ProfileTraderTemplate
{
    [JsonPropertyName("initialLoyaltyLevel")]
    public Dictionary<string, int?>? InitialLoyaltyLevel
    {
        get;
        set;
    }

    [JsonPropertyName("initialStanding")]
    public Dictionary<string, double?>? InitialStanding
    {
        get;
        set;
    }

    [JsonPropertyName("setQuestsAvailableForStart")]
    public bool? SetQuestsAvailableForStart
    {
        get;
        set;
    }

    [JsonPropertyName("setQuestsAvailableForFinish")]
    public bool? SetQuestsAvailableForFinish
    {
        get;
        set;
    }

    [JsonPropertyName("initialSalesSum")]
    public int? InitialSalesSum
    {
        get;
        set;
    }

    [JsonPropertyName("jaegerUnlocked")]
    public bool? JaegerUnlocked
    {
        get;
        set;
    }

    /// <summary>
    ///     How many days is usage of the flea blocked for upon profile creation
    /// </summary>
    [JsonPropertyName("fleaBlockedDays")]
    public int? FleaBlockedDays
    {
        get;
        set;
    }

    /// <summary>
    ///     What traders default to being locked on profile creation
    /// </summary>
    [JsonPropertyName("lockedByDefaultOverride")]
    public List<string>? LockedByDefaultOverride
    {
        get;
        set;
    }

    /// <summary>
    ///     What traders should have their clothing unlocked/purchased on creation
    /// </summary>
    [JsonPropertyName("purchaseAllClothingByDefaultForTrader")]
    public List<string>? PurchaseAllClothingByDefaultForTrader
    {
        get;
        set;
    }
}
