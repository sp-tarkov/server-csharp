using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Services;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Trader
{
    [JsonPropertyName("assort")]
    public TraderAssort? Assort { get; set; }

    [JsonPropertyName("base")]
    public TraderBase? Base { get; set; }

    [JsonPropertyName("dialogue")]
    public Dictionary<string, List<string>?>? Dialogue { get; set; }

    [JsonPropertyName("questassort")]
    public Dictionary<string, Dictionary<string, string>>? QuestAssort { get; set; }

    [JsonPropertyName("suits")]
    public List<Suit>? Suits { get; set; }

    [JsonPropertyName("services")]
    public List<TraderServiceModel>? Services { get; set; }
}

public record TraderBase
{
    [JsonPropertyName("refreshTraderRagfairOffers")]
    public bool? RefreshTraderRagfairOffers { get; set; }

    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("availableInRaid")]
    public bool? AvailableInRaid { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("balance_dol")]
    public decimal? BalanceDollar { get; set; }

    [JsonPropertyName("balance_eur")]
    public decimal? BalanceEuro { get; set; }

    [JsonPropertyName("balance_rub")]
    public decimal? BalanceRub { get; set; }

    [JsonPropertyName("buyer_up")]
    public bool? BuyerUp { get; set; }

    [JsonPropertyName("currency")]
    public CurrencyType? Currency { get; set; }

    [JsonPropertyName("customization_seller")]
    public bool? CustomizationSeller { get; set; }

    [JsonPropertyName("discount")]
    public decimal? Discount { get; set; }

    [JsonPropertyName("discount_end")]
    public decimal? DiscountEnd { get; set; }

    [JsonPropertyName("gridHeight")]
    public double? GridHeight { get; set; }

    [JsonPropertyName("sell_modifier_for_prohibited_items")]
    public int? ProhibitedItemsSellModifier { get; set; }

    [JsonPropertyName("insurance")]
    public TraderInsurance? Insurance { get; set; }

    [JsonPropertyName("items_buy")]
    public ItemBuyData? ItemsBuy { get; set; }

    [JsonPropertyName("items_buy_prohibited")]
    public ItemBuyData? ItemsBuyProhibited { get; set; }

    [JsonPropertyName("isCanTransferItems")]
    public bool? IsCanTransferItems { get; set; }

    [JsonPropertyName("transferableItems")]
    public ItemBuyData? TransferableItems { get; set; }

    [JsonPropertyName("prohibitedTransferableItems")]
    public ItemBuyData? ProhibitedTransferableItems { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("loyaltyLevels")]
    public List<TraderLoyaltyLevel>? LoyaltyLevels { get; set; }

    [JsonPropertyName("medic")]
    public bool? Medic { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    // Confirmed in client
    [JsonPropertyName("nextResupply")]
    public int? NextResupply { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("repair")]
    public TraderRepair? Repair { get; set; }

    [JsonPropertyName("sell_category")]
    public List<string>? SellCategory { get; set; }

    [JsonPropertyName("surname")]
    public string? Surname { get; set; }

    [JsonPropertyName("unlockedByDefault")]
    public bool? UnlockedByDefault { get; set; }
}

public record ItemBuyData
{
    // MongoId
    [JsonPropertyName("category")]
    public List<string>? Category { get; set; }

    // MongoId
    [JsonPropertyName("id_list")]
    public List<string>? IdList { get; set; }
}

public record TraderInsurance
{
    [JsonPropertyName("availability")]
    public bool? Availability { get; set; }

    // MongoId
    [JsonPropertyName("excluded_category")]
    public List<string>? ExcludedCategory { get; set; }

    // Confirmed in client
    [JsonPropertyName("max_return_hour")]
    public int? MaxReturnHour { get; set; }

    [JsonPropertyName("max_storage_time")]
    public double? MaxStorageTime { get; set; }

    // Confirmed in client
    [JsonPropertyName("min_payment")]
    public int? MinPayment { get; set; }

    // Confirmed in client
    [JsonPropertyName("min_return_hour")]
    public int? MinReturnHour { get; set; }
}

public record TraderLoyaltyLevel
{
    [JsonPropertyName("buy_price_coef")]
    public double? BuyPriceCoefficient { get; set; }

    [JsonPropertyName("exchange_price_coef")]
    public double? ExchangePriceCoefficient { get; set; }

    [JsonPropertyName("heal_price_coef")]
    public double? HealPriceCoefficient { get; set; }

    [JsonPropertyName("insurance_price_coef")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public double? InsurancePriceCoefficient { get; set; }

    // Chceked on client
    [JsonPropertyName("minLevel")]
    public int? MinLevel { get; set; }

    [JsonPropertyName("minSalesSum")]
    public long? MinSalesSum { get; set; }

    [JsonPropertyName("minStanding")]
    public double? MinStanding { get; set; }

    [JsonPropertyName("repair_price_coef")]
    public double? RepairPriceCoefficient { get; set; }
}

public record TraderRepair
{
    [JsonPropertyName("availability")]
    public bool? Availability { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("currency_coefficient")]
    public double? CurrencyCoefficient { get; set; }

    [JsonPropertyName("excluded_category")]
    public List<string>? ExcludedCategory { get; set; }

    /// <summary>
    ///     Doesn't exist in client object
    /// </summary>
    [JsonPropertyName("excluded_id_list")]
    public List<string>? ExcludedIdList { get; set; }

    [JsonPropertyName("quality")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public double? Quality { get; set; }

    [JsonPropertyName("price_rate")]
    public double? PriceRate { get; set; }
}

public record TraderAssort
{
    [JsonPropertyName("nextResupply")]
    public double? NextResupply { get; set; }

    [JsonPropertyName("items")]
    public List<Item>? Items { get; set; }

    [JsonPropertyName("barter_scheme")]
    public Dictionary<string, List<List<BarterScheme>>>? BarterScheme { get; set; }

    [JsonPropertyName("loyal_level_items")]
    public Dictionary<string, int>? LoyalLevelItems { get; set; }
}

public record BarterScheme
{
    // Confirmed in client
    [JsonPropertyName("count")]
    public double? Count { get; set; }

    [JsonPropertyName("_tpl")]
    public string? Template { get; set; }

    [JsonPropertyName("onlyFunctional")]
    public bool? OnlyFunctional { get; set; }

    [JsonPropertyName("sptQuestLocked")]
    public bool? SptQuestLocked { get; set; }

    [JsonPropertyName("level")]
    public int? Level { get; set; }

    [JsonPropertyName("side")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DogtagExchangeSide? Side { get; set; }
}

public record Suit
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("externalObtain")]
    public bool? ExternalObtain { get; set; }

    [JsonPropertyName("internalObtain")]
    public bool? InternalObtain { get; set; }

    [JsonPropertyName("isHiddenInPVE")]
    public bool? IsHiddenInPVE { get; set; }

    [JsonPropertyName("tid")]
    public string? Tid { get; set; }

    [JsonPropertyName("suiteId")]
    public string? SuiteId { get; set; }

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("requirements")]
    public SuitRequirements? Requirements { get; set; }

    [JsonPropertyName("relatedBattlePassSeason")]
    public int? RelatedBattlePassSeason { get; set; }
}

public record SuitRequirements
{
    [JsonPropertyName("achievementRequirements")]
    public List<string>? AchievementRequirements { get; set; }

    [JsonPropertyName("loyaltyLevel")]
    public double? LoyaltyLevel { get; set; }

    [JsonPropertyName("profileLevel")]
    public double? ProfileLevel { get; set; }

    // Checked in client
    [JsonPropertyName("standing")]
    public double? Standing { get; set; }

    [JsonPropertyName("skillRequirements")]
    public List<string>? SkillRequirements { get; set; }

    [JsonPropertyName("questRequirements")]
    public List<string>? QuestRequirements { get; set; }

    [JsonPropertyName("itemRequirements")]
    public List<ItemRequirement>? ItemRequirements { get; set; }

    [JsonPropertyName("requiredTid")]
    public string? RequiredTid { get; set; }
}

public record ItemRequirement
{
    [JsonPropertyName("count")]
    public double? Count { get; set; }

    [JsonPropertyName("_tpl")]
    public string? Tpl { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("onlyFunctional")]
    public bool? OnlyFunctional { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
