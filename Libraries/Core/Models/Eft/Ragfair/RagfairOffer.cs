using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;

namespace Core.Models.Eft.Ragfair;

public record RagfairOffer
{
    [JsonPropertyName("sellResult")]
    public List<SellResult>? SellResults { get; set; }

    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("items")]
    public List<Item>? Items { get; set; }

    [JsonPropertyName("requirements")]
    public List<OfferRequirement>? Requirements { get; set; }

    [JsonPropertyName("root")]
    public string? Root { get; set; }

    [JsonPropertyName("intId")]
    public int? InternalId { get; set; }

    /** Handbook price */
    [JsonPropertyName("itemsCost")]
    public decimal? ItemsCost { get; set; }

    /** Rouble price per item */
    [JsonPropertyName("requirementsCost")]
    public double? RequirementsCost { get; set; }

    [JsonPropertyName("startTime")]
    public long? StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public long? EndTime { get; set; }

    /** True when offer is sold as pack */
    [JsonPropertyName("sellInOnePiece")]
    public bool? SellInOnePiece { get; set; }

    /** Rouble price - same as requirementsCost */
    [JsonPropertyName("summaryCost")]
    public double? SummaryCost { get; set; }

    [JsonPropertyName("user")]
    public RagfairOfferUser? User { get; set; }

    /** Trader only */
    [JsonPropertyName("unlimitedCount")]
    public bool? UnlimitedCount { get; set; }

    [JsonPropertyName("loyaltyLevel")]
    public int? LoyaltyLevel { get; set; }

    [JsonPropertyName("buyRestrictionMax")]
    public int? BuyRestrictionMax { get; set; }

    [JsonPropertyName("buyRestrictionCurrent")]
    public int? BuyRestrictionCurrent { get; set; }

    [JsonPropertyName("locked")]
    public bool? Locked { get; set; }
}

public record OfferRequirement
{
    [JsonPropertyName("_tpl")]
    public string? Template { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("onlyFunctional")]
    public bool? OnlyFunctional { get; set; }

    [JsonPropertyName("level")]
    public int? Level { get; set; }

    [JsonPropertyName("side")]
    public DogtagExchangeSide? Side { get; set; }
}

public record RagfairOfferUser
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("rating")]
    public decimal? Rating { get; set; }

    [JsonPropertyName("memberType")]
    public MemberCategory? MemberType { get; set; }

    [JsonPropertyName("selectedMemberCategory")]
    public MemberCategory? SelectedMemberCategory { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("isRatingGrowing")]
    public bool? IsRatingGrowing { get; set; }

    [JsonPropertyName("aid")]
    public int? Aid { get; set; }
}

public record SellResult
{
    [JsonPropertyName("sellTime")]
    public long? SellTime { get; set; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }
}
