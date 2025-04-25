using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record RagFair
{
    [JsonPropertyName("enabled")]
    public bool? Enabled
    {
        get;
        set;
    }

    [JsonPropertyName("priceStabilizerEnabled")]
    public bool? PriceStabilizerEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("includePveTraderSales")]
    public bool? IncludePveTraderSales
    {
        get;
        set;
    }

    [JsonPropertyName("priceStabilizerStartIntervalInHours")]
    public double? PriceStabilizerStartIntervalInHours
    {
        get;
        set;
    }

    // Checked in client
    [JsonPropertyName("minUserLevel")]
    public int? MinUserLevel
    {
        get;
        set;
    }

    [JsonPropertyName("communityTax")]
    public float? CommunityTax
    {
        get;
        set;
    }

    [JsonPropertyName("communityItemTax")]
    public float? CommunityItemTax
    {
        get;
        set;
    }

    // Checked in client
    [JsonPropertyName("communityRequirementTax")]
    public double? CommunityRequirementTax
    {
        get;
        set;
    }

    [JsonPropertyName("offerPriorityCost")]
    public float? OfferPriorityCost
    {
        get;
        set;
    }

    [JsonPropertyName("offerDurationTimeInHour")]
    public double? OfferDurationTimeInHour
    {
        get;
        set;
    }

    [JsonPropertyName("offerDurationTimeInHourAfterRemove")]
    public double? OfferDurationTimeInHourAfterRemove
    {
        get;
        set;
    }

    [JsonPropertyName("priorityTimeModifier")]
    public float? PriorityTimeModifier
    {
        get;
        set;
    }

    [JsonPropertyName("maxRenewOfferTimeInHour")]
    public double? MaxRenewOfferTimeInHour
    {
        get;
        set;
    }

    [JsonPropertyName("renewPricePerHour")]
    public float? RenewPricePerHour
    {
        get;
        set;
    }

    [JsonPropertyName("maxActiveOfferCount")]
    public List<MaxActiveOfferCount>? MaxActiveOfferCount
    {
        get;
        set;
    }

    [JsonPropertyName("balancerRemovePriceCoefficient")]
    public float? BalancerRemovePriceCoefficient
    {
        get;
        set;
    }

    [JsonPropertyName("balancerMinPriceCount")]
    public float? BalancerMinPriceCount
    {
        get;
        set;
    }

    [JsonPropertyName("balancerAveragePriceCoefficient")]
    public float? BalancerAveragePriceCoefficient
    {
        get;
        set;
    }

    [JsonPropertyName("delaySinceOfferAdd")]
    public int? DelaySinceOfferAdd
    {
        get;
        set;
    }

    [JsonPropertyName("uniqueBuyerTimeoutInDays")]
    public double? UniqueBuyerTimeoutInDays
    {
        get;
        set;
    }

    [JsonPropertyName("userRatingChangeFrequencyMultiplayer")]
    public float? UserRatingChangeFrequencyMultiplayer
    {
        get;
        set;
    }

    [JsonPropertyName("RagfairTurnOnTimestamp")]
    public long? RagfairTurnOnTimestamp
    {
        get;
        set;
    }

    [JsonPropertyName("ratingSumForIncrease")]
    public double? RatingSumForIncrease
    {
        get;
        set;
    }

    [JsonPropertyName("ratingIncreaseCount")]
    public double? RatingIncreaseCount
    {
        get;
        set;
    }

    [JsonPropertyName("ratingSumForDecrease")]
    public double? RatingSumForDecrease
    {
        get;
        set;
    }

    [JsonPropertyName("ratingDecreaseCount")]
    public double? RatingDecreaseCount
    {
        get;
        set;
    }

    [JsonPropertyName("maxSumForIncreaseRatingPerOneSale")]
    public double? MaxSumForIncreaseRatingPerOneSale
    {
        get;
        set;
    }

    [JsonPropertyName("maxSumForDecreaseRatingPerOneSale")]
    public double? MaxSumForDecreaseRatingPerOneSale
    {
        get;
        set;
    }

    [JsonPropertyName("maxSumForRarity")]
    public MaxSumForRarity? MaxSumForRarity
    {
        get;
        set;
    }

    [JsonPropertyName("ChangePriceCoef")]
    public double? ChangePriceCoef
    {
        get;
        set;
    }

    [JsonPropertyName("ItemRestrictions")]
    public List<ItemGlobalRestrictions>? ItemRestrictions
    {
        get;
        set;
    }

    [JsonPropertyName("balancerUserItemSaleCooldownEnabled")]
    public bool? BalancerUserItemSaleCooldownEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("balancerUserItemSaleCooldown")]
    public float? BalancerUserItemSaleCooldown
    {
        get;
        set;
    }

    [JsonPropertyName("youSellOfferMaxStorageTimeInHour")]
    public double? YouSellOfferMaxStorageTimeInHour
    {
        get;
        set;
    }

    [JsonPropertyName("yourOfferDidNotSellMaxStorageTimeInHour")]
    public double? YourOfferDidNotSellMaxStorageTimeInHour
    {
        get;
        set;
    }

    [JsonPropertyName("isOnlyFoundInRaidAllowed")]
    public bool? IsOnlyFoundInRaidAllowed
    {
        get;
        set;
    }

    [JsonPropertyName("sellInOnePiece")]
    public double? SellInOnePiece
    {
        get;
        set;
    }
}
