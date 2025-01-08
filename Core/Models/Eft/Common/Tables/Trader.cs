using System.Text.Json.Serialization;
using Core.Models.Enums;
using Core.Models.Spt.Services;
using Core.Utils.Json.Converters;

namespace Core.Models.Eft.Common.Tables;

public class Trader
{
    [JsonPropertyName("assort")]
    public TraderAssort? Assort { get; set; }

    [JsonPropertyName("base")]
    public TraderBase? Base { get; set; }

    [JsonPropertyName("dialogue")]
    public Dictionary<string, List<string>>? Dialogue { get; set; }

    [JsonPropertyName("questassort")]
    public Dictionary<string, Dictionary<string, string>>? QuestAssort { get; set; }

    [JsonPropertyName("suits")]
    public List<Suit>? Suits { get; set; }

    [JsonPropertyName("services")]
    public List<TraderServiceModel>? Services { get; set; }
}

public class TraderBase
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
    public string? Currency { get; set; }

    [JsonPropertyName("customization_seller")]
    public bool? CustomizationSeller { get; set; }

    [JsonPropertyName("discount")]
    public decimal? Discount { get; set; }

    [JsonPropertyName("discount_end")]
    public decimal? DiscountEnd { get; set; }

    [JsonPropertyName("gridHeight")]
    public double? GridHeight { get; set; }

    [JsonPropertyName("sell_modifier_for_prohibited_items")]
    public decimal? SellModifierForProhibitedItems { get; set; }

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

    [JsonPropertyName("nextResupply")]
    public double? NextResupply { get; set; }

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

public class ItemBuyData
{
    [JsonPropertyName("category")]
    public List<string>? Category { get; set; }

    [JsonPropertyName("id_list")]
    public List<string>? IdList { get; set; }
}

public class TraderInsurance
{
    [JsonPropertyName("availability")]
    public bool? Availability { get; set; }

    [JsonPropertyName("excluded_category")]
    public List<string>? ExcludedCategory { get; set; }

    [JsonPropertyName("max_return_hour")]
    public double? MaxReturnHour { get; set; }

    [JsonPropertyName("max_storage_time")]
    public double? MaxStorageTime { get; set; }

    [JsonPropertyName("min_payment")]
    public double? MinPayment { get; set; }

    [JsonPropertyName("min_return_hour")]
    public double? MinReturnHour { get; set; }
}

public class TraderLoyaltyLevel
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

    [JsonPropertyName("minLevel")]
    public double? MinLevel { get; set; }

    [JsonPropertyName("minSalesSum")]
    public double? MinSalesSum { get; set; }

    [JsonPropertyName("minStanding")]
    public double? MinStanding { get; set; }

    [JsonPropertyName("repair_price_coef")]
    public double? RepairPriceCoefficient { get; set; }
}

public class TraderRepair
{
    [JsonPropertyName("availability")]
    public bool? Availability { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("currency_coefficient")]
    public double? CurrencyCoefficient { get; set; }

    [JsonPropertyName("excluded_category")]
    public List<string>? ExcludedCategory { get; set; }

    [JsonPropertyName("excluded_id_list")]
    public List<string>? ExcludedIdList { get; set; } // Doesn't exist in client object

    [JsonPropertyName("quality")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public double? Quality { get; set; }
    
    [JsonPropertyName("price_rate")]
    public double? PriceRate { get; set; }

}

public class TraderAssort
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

public class BarterScheme
{
    [JsonPropertyName("count")]
    public double? Count { get; set; }

    [JsonPropertyName("_tpl")]
    public string? Template { get; set; }

    [JsonPropertyName("onlyFunctional")]
    public bool? OnlyFunctional { get; set; }

    [JsonPropertyName("sptQuestLocked")]
    public bool? SptQuestLocked { get; set; }

    [JsonPropertyName("level")]
    public double? Level { get; set; }

    [JsonPropertyName("side")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DogtagExchangeSide? Side { get; set; }
}

public class Suit
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
}

public class SuitRequirements
{
    [JsonPropertyName("achievementRequirements")]
    public List<string>? AchievementRequirements { get; set; }

    [JsonPropertyName("loyaltyLevel")]
    public double? LoyaltyLevel { get; set; }

    [JsonPropertyName("profileLevel")]
    public double? ProfileLevel { get; set; }

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

public class ItemRequirement
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
