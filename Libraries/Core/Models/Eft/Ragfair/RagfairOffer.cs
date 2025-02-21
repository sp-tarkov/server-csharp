using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;

namespace Core.Models.Eft.Ragfair;

public record RagfairOffer
{
    private string? _id;

    private string? _root;

    [JsonPropertyName("sellResult")]
    public List<SellResult>? SellResults
    {
        get;
        set;
    }

    [JsonPropertyName("_id")]
    public string? Id
    {
        get
        {
            return _id;
        }
        set
        {
            _id = string.Intern(value);
        }
    }

    [JsonPropertyName("items")]
    public List<Item>? Items
    {
        get;
        set;
    }

    [JsonPropertyName("requirements")]
    public List<OfferRequirement>? Requirements
    {
        get;
        set;
    }

    [JsonPropertyName("root")]
    public string? Root
    {
        get
        {
            return _root;
        }
        set
        {
            _root = string.Intern(value);
        }
    }

    [JsonPropertyName("intId")]
    public int? InternalId
    {
        get;
        set;
    }

    /**
     * Handbook price
     */
    [JsonPropertyName("itemsCost")]
    public double? ItemsCost
    {
        get;
        set;
    }

    /**
     * Rouble price per item
     */
    [JsonPropertyName("requirementsCost")]
    public double? RequirementsCost
    {
        get;
        set;
    }

    [JsonPropertyName("startTime")]
    public long? StartTime
    {
        get;
        set;
    }

    [JsonPropertyName("endTime")]
    public long? EndTime
    {
        get;
        set;
    }

    /**
     * True when offer is sold as pack
     */
    [JsonPropertyName("sellInOnePiece")]
    public bool? SellInOnePiece
    {
        get;
        set;
    }

    /**
     * Rouble price - same as requirementsCost
     */
    [JsonPropertyName("summaryCost")]
    public double? SummaryCost
    {
        get;
        set;
    }

    [JsonPropertyName("user")]
    public RagfairOfferUser? User
    {
        get;
        set;
    }

    /**
     * Trader only
     */
    [JsonPropertyName("unlimitedCount")]
    public bool? UnlimitedCount
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

    [JsonPropertyName("buyRestrictionMax")]
    public int? BuyRestrictionMax
    {
        get;
        set;
    }

    // Confirmed in client
    [JsonPropertyName("buyRestrictionCurrent")]
    public int? BuyRestrictionCurrent
    {
        get;
        set;
    }

    [JsonPropertyName("locked")]
    public bool? Locked
    {
        get;
        set;
    }

    /// <summary>
    /// Tightly bound to offer.items[0].upd.stackObjectsCount
    /// </summary>
    [JsonPropertyName("quantity")]
    public int? Quantity
    {
        get;
        set;
    }
}

public record OfferRequirement
{
    private string? _tpl;

    [JsonPropertyName("_tpl")]
    public string? Template
    {
        get
        {
            return _tpl;
        }
        set
        {
            _tpl = string.Intern(value);
        }
    }

    [JsonPropertyName("count")]
    public double? Count
    {
        get;
        set;
    }

    [JsonPropertyName("onlyFunctional")]
    public bool? OnlyFunctional
    {
        get;
        set;
    }

    [JsonPropertyName("level")]
    public int? Level
    {
        get;
        set;
    }

    [JsonPropertyName("side")]
    public DogtagExchangeSide? Side
    {
        get;
        set;
    }
}

public record RagfairOfferUser
{
    private string? _id;

    [JsonPropertyName("id")]
    public string? Id
    {
        get
        {
            return _id;
        }
        set
        {
            _id = string.Intern(value);
        }
    }

    [JsonPropertyName("nickname")]
    public string? Nickname
    {
        get;
        set;
    }

    [JsonPropertyName("rating")]
    public double? Rating
    {
        get;
        set;
    }

    [JsonPropertyName("memberType")]
    public MemberCategory? MemberType
    {
        get;
        set;
    }

    [JsonPropertyName("selectedMemberCategory")]
    public MemberCategory? SelectedMemberCategory
    {
        get;
        set;
    }

    [JsonPropertyName("avatar")]
    public string? Avatar
    {
        get;
        set;
    }

    [JsonPropertyName("isRatingGrowing")]
    public bool? IsRatingGrowing
    {
        get;
        set;
    }

    [JsonPropertyName("aid")]
    public int? Aid
    {
        get;
        set;
    }
}

public record SellResult
{
    [JsonPropertyName("sellTime")]
    public long? SellTime
    {
        get;
        set;
    }

    [JsonPropertyName("amount")]
    public int? Amount
    {
        get;
        set;
    }
}
