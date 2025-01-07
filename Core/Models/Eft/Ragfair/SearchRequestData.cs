using System.Text.Json.Serialization;
using Core.Models.Enums;

namespace Core.Models.Eft.Ragfair;

public class SearchRequestData
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("sortType")]
    public RagfairSort SortType { get; set; }

    [JsonPropertyName("sortDirection")]
    public int SortDirection { get; set; }

    [JsonPropertyName("currency")]
    public int Currency { get; set; }

    [JsonPropertyName("priceFrom")]
    public int PriceFrom { get; set; }

    [JsonPropertyName("priceTo")]
    public int PriceTo { get; set; }

    [JsonPropertyName("quantityFrom")]
    public int QuantityFrom { get; set; }

    [JsonPropertyName("quantityTo")]
    public int QuantityTo { get; set; }

    [JsonPropertyName("conditionFrom")]
    public int ConditionFrom { get; set; }

    [JsonPropertyName("conditionTo")]
    public int ConditionTo { get; set; }

    [JsonPropertyName("oneHourExpiration")]
    public bool OneHourExpiration { get; set; }

    [JsonPropertyName("removeBartering")]
    public bool RemoveBartering { get; set; }

    [JsonPropertyName("offerOwnerType")]
    public OfferOwnerType OfferOwnerType { get; set; }

    [JsonPropertyName("onlyFunctional")]
    public bool OnlyFunctional { get; set; }

    [JsonPropertyName("updateOfferCount")]
    public bool UpdateOfferCount { get; set; }

    [JsonPropertyName("handbookId")]
    public string HandbookId { get; set; }

    [JsonPropertyName("linkedSearchId")]
    public string LinkedSearchId { get; set; }

    [JsonPropertyName("neededSearchId")]
    public string NeededSearchId { get; set; }

    [JsonPropertyName("buildItems")]
    public BuildItems BuildItems { get; set; }

    [JsonPropertyName("buildCount")]
    public int BuildCount { get; set; }

    [JsonPropertyName("tm")]
    public int Tm { get; set; }

    [JsonPropertyName("reload")]
    public int Reload { get; set; }
}

public enum OfferOwnerType
{
    ANYOWNERTYPE = 0,
    TRADEROWNERTYPE = 1,
    PLAYEROWNERTYPE = 2,
}

public class BuildItems
{
    // Define properties for BuildItems here if needed
}