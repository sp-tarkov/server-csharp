using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record RagfairConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind
    {
        get;
        set;
    } = "spt-ragfair";

    /// <summary>
    /// How many seconds should pass before expired offers and processed + player offers checked if sold
    /// </summary>
    [JsonPropertyName("runIntervalSeconds")]
    public int RunIntervalSeconds
    {
        get;
        set;
    }

    /// <summary>
    /// Default values used to hydrate `runIntervalSeconds` with
    /// </summary>
    [JsonPropertyName("runIntervalValues")]
    public RunIntervalValues RunIntervalValues
    {
        get;
        set;
    }

    /// <summary>
    /// Player listing settings
    /// </summary>
    [JsonPropertyName("sell")]
    public Sell Sell
    {
        get;
        set;
    }

    /// <summary>
    /// Trader ids + should their assorts be listed on flea
    /// </summary>
    [JsonPropertyName("traders")]
    public Dictionary<string, bool> Traders
    {
        get;
        set;
    }

    [JsonPropertyName("dynamic")]
    public Dynamic Dynamic
    {
        get;
        set;
    }

    [JsonPropertyName("tieredFlea")]
    public TieredFlea TieredFlea
    {
        get;
        set;
    }
}

public record Sell
{
    /// <summary>
    /// Should a fee be deducted from player when listing an item for sale
    /// </summary>
    [JsonPropertyName("fees")]
    public bool Fees
    {
        get;
        set;
    }

    /// <summary>
    /// Settings to control chances of offer being sold
    /// </summary>
    [JsonPropertyName("chance")]
    public Chance Chance
    {
        get;
        set;
    }

    /// <summary>
    /// Settings to control how long it takes for a player offer to sell
    /// </summary>
    [JsonPropertyName("time")]
    public MinMax<double> Time
    {
        get;
        set;
    }

    /// <summary>
    /// Seconds from clicking remove to remove offer from market
    /// </summary>
    [JsonPropertyName("expireSeconds")]
    public int ExpireSeconds
    {
        get;
        set;
    }
}

public record Chance
{
    /// <summary>
    /// Base chance percent to sell an item
    /// </summary>
    [JsonPropertyName("base")]
    public int Base
    {
        get;
        set;
    }

    /// <summary>
    /// Value to multiply the sell chance by
    /// </summary>
    [JsonPropertyName("sellMultiplier")]
    public double SellMultiplier
    {
        get;
        set;
    }

    /// <summary>
    /// Max possible sell chance % for a player listed offer
    /// </summary>
    [JsonPropertyName("maxSellChancePercent")]
    public int MaxSellChancePercent
    {
        get;
        set;
    }

    /// <summary>
    /// Min possible sell chance % for a player listed offer
    /// </summary>
    [JsonPropertyName("minSellChancePercent")]
    public int MinSellChancePercent
    {
        get;
        set;
    }
}

public record Dynamic
{
    /// <summary>
    /// Should a purchased dynamic offers items be flagged as found in raid
    /// </summary>
    [JsonPropertyName("purchasesAreFoundInRaid")]
    public bool PurchasesAreFoundInRaid
    {
        get;
        set;
    }

    /// <summary>
    /// Use the highest trader price for an offer if its greater than the price in templates/prices.json
    /// </summary>
    [JsonPropertyName("useTraderPriceForOffersIfHigher")]
    public bool UseTraderPriceForOffersIfHigher
    {
        get;
        set;
    }

    /// <summary>
    /// Barter offer specific settings
    /// </summary>
    [JsonPropertyName("barter")]
    public BarterDetails Barter
    {
        get;
        set;
    }

    [JsonPropertyName("pack")]
    public PackDetails Pack
    {
        get;
        set;
    }

    /// <summary>
    /// Dynamic offer price below handbook adjustment values
    /// </summary>
    [JsonPropertyName("offerAdjustment")]
    public OfferAdjustment OfferAdjustment
    {
        get;
        set;
    }

    /// <summary>
    /// How many offers should expire before an offer regeneration occurs
    /// </summary>
    [JsonPropertyName("expiredOfferThreshold")]
    public int ExpiredOfferThreshold
    {
        get;
        set;
    }

    /// <summary>
    /// How many offers should be listed
    /// </summary>
    [JsonPropertyName("offerItemCount")]
    public MinMax<int> OfferItemCount
    {
        get;
        set;
    }

    /// <summary>
    /// How much should the price of an offer vary by (percent 0.8 = 80%, 1.2 = 120%)
    /// </summary>
    [JsonPropertyName("priceRanges")]
    public PriceRanges PriceRanges
    {
        get;
        set;
    }

    /// <summary>
    /// Should default presets to listed only or should non-standard presets found in globals.json be listed too
    /// </summary>
    [JsonPropertyName("showDefaultPresetsOnly")]
    public bool ShowDefaultPresetsOnly
    {
        get;
        set;
    }

    /// <summary>
    /// Tpls that should not use the variable price system when their quality is less than 100% (lower dura/uses = lower price)
    /// </summary>
    [JsonPropertyName("ignoreQualityPriceVarianceBlacklist")]
    public HashSet<string> IgnoreQualityPriceVarianceBlacklist
    {
        get;
        set;
    }

    [JsonPropertyName("endTimeSeconds")]
    public MinMax<int> EndTimeSeconds
    {
        get;
        set;
    }

    /// <summary>
    /// Settings to control the durability range of item items listed on flea
    /// </summary>
    [JsonPropertyName("condition")]
    public Dictionary<string, Condition> Condition
    {
        get;
        set;
    }

    /// <summary>
    /// Size stackable items should be listed for in percent of max stack size
    /// </summary>
    [JsonPropertyName("stackablePercent")]
    public MinMax<double> StackablePercent
    {
        get;
        set;
    }

    /// <summary>
    /// Items that cannot be stacked can have multiples sold in one offer, what range of values can be listed
    /// </summary>
    [JsonPropertyName("nonStackableCount")]
    public MinMax<int> NonStackableCount
    {
        get;
        set;
    }

    /// <summary>
    /// Range of rating offers for items being listed
    /// </summary>
    [JsonPropertyName("rating")]
    public MinMax<double> Rating
    {
        get;
        set;
    }

    /// <summary>
    /// Armor specific flea settings
    /// </summary>
    [JsonPropertyName("armor")]
    public ArmorSettings Armor
    {
        get;
        set;
    }

    /// <summary>
    /// A multipler to apply to individual tpls price just prior to item quality adjustment
    /// </summary>
    [JsonPropertyName("itemPriceMultiplier")]
    public Dictionary<string, double>? ItemPriceMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("_currencies")]
    public string? CurrenciesDescription
    {
        get;
        set;
    }

    /// <summary>
    /// Percentages to sell offers in each currency
    /// </summary>
    [JsonPropertyName("currencies")]
    public Dictionary<string, double> Currencies
    {
        get;
        set;
    }

    /// <summary>
    /// Item tpls that should be forced to sell as a single item
    /// </summary>
    [JsonPropertyName("showAsSingleStack")]
    public HashSet<string> ShowAsSingleStack
    {
        get;
        set;
    }

    /// <summary>
    /// Should christmas/halloween items be removed from flea when not within the seasonal bounds
    /// </summary>
    [JsonPropertyName("removeSeasonalItemsWhenNotInEvent")]
    public bool RemoveSeasonalItemsWhenNotInEvent
    {
        get;
        set;
    }

    /// <summary>
    /// Flea blacklist settings
    /// </summary>
    [JsonPropertyName("blacklist")]
    public RagfairBlacklist Blacklist
    {
        get;
        set;
    }

    /// <summary>
    /// Dict of price limits keyed by item type
    /// </summary>
    [JsonPropertyName("unreasonableModPrices")]
    public Dictionary<string, UnreasonableModPrices> UnreasonableModPrices
    {
        get;
        set;
    }

    /// <summary>
    /// Custom rouble prices for items to override values from prices.json
    /// </summary>
    [JsonPropertyName("itemPriceOverrideRouble")]
    public Dictionary<string, double> ItemPriceOverrideRouble
    {
        get;
        set;
    }
}

public record PriceRanges
{
    [JsonPropertyName("default")]
    public MinMax<double> Default
    {
        get;
        set;
    }

    [JsonPropertyName("preset")]
    public MinMax<double> Preset
    {
        get;
        set;
    }

    [JsonPropertyName("pack")]
    public MinMax<double> Pack
    {
        get;
        set;
    }
}

public record BarterDetails
{
    /// <summary>
    /// Percentage change an offer is listed as a barter
    /// </summary>
    [JsonPropertyName("chancePercent")]
    public double ChancePercent
    {
        get;
        set;
    }

    /// <summary>
    /// Min number of required items for a barter requirement
    /// </summary>
    [JsonPropertyName("itemCountMin")]
    public int ItemCountMin
    {
        get;
        set;
    }

    /// <summary>
    /// Max number of required items for a barter requirement
    /// </summary>
    [JsonPropertyName("itemCountMax")]
    public int ItemCountMax
    {
        get;
        set;
    }

    /// <summary>
    /// How much can the total price of requested items vary from the item offered
    /// </summary>
    [JsonPropertyName("priceRangeVariancePercent")]
    public double PriceRangeVariancePercent
    {
        get;
        set;
    }

    /// <summary>
    /// Min rouble price for an offer to be considered for turning into a barter
    /// </summary>
    [JsonPropertyName("minRoubleCostToBecomeBarter")]
    public double MinRoubleCostToBecomeBarter
    {
        get;
        set;
    }

    /// <summary>
    /// Should barter offers only single stack
    /// </summary>
    [JsonPropertyName("makeSingleStackOnly")]
    public bool MakeSingleStackOnly
    {
        get;
        set;
    }

    /// <summary>
    /// Item Tpls to never be turned into a barter
    /// </summary>
    [JsonPropertyName("itemTypeBlacklist")]
    public HashSet<string> ItemTypeBlacklist
    {
        get;
        set;
    }
}

public record PackDetails
{
    /// <summary>
    /// Percentage change an offer is listed as a pack
    /// </summary>
    [JsonPropertyName("chancePercent")]
    public double ChancePercent
    {
        get;
        set;
    }

    /// <summary>
    /// Min number of required items for a pack
    /// </summary>
    [JsonPropertyName("itemCountMin")]
    public int ItemCountMin
    {
        get;
        set;
    }

    /// <summary>
    /// Max number of required items for a pack
    /// </summary>
    [JsonPropertyName("itemCountMax")]
    public int ItemCountMax
    {
        get;
        set;
    }

    /// <summary>
    /// item types to allow being a pack
    /// </summary>
    [JsonPropertyName("itemTypeWhitelist")]
    public HashSet<string> ItemTypeWhitelist
    {
        get;
        set;
    }
}

public record OfferAdjustment
{
    /// <summary>
    ///     Shuld offer price be adjusted when below handbook price
    /// </summary>
    [JsonPropertyName("adjustPriceWhenBelowHandbookPrice")]
    public bool AdjustPriceWhenBelowHandbookPrice
    {
        get;
        set;
    }

    /// <summary>
    ///     How big a percentage difference does price need to vary from handbook to be considered for adjustment
    /// </summary>
    [JsonPropertyName("maxPriceDifferenceBelowHandbookPercent")]
    public double MaxPriceDifferenceBelowHandbookPercent
    {
        get;
        set;
    }

    /// <summary>
    ///     How much to multiply the handbook price to get the new price
    /// </summary>
    [JsonPropertyName("handbookPriceMultipier")]
    public double HandbookPriceMultiplier
    {
        get;
        set;
    }

    /// <summary>
    ///     What is the minimum rouble price to consider adjusting price of item
    /// </summary>
    [JsonPropertyName("priceThreshholdRub")]
    public double PriceThresholdRub
    {
        get;
        set;
    }
}

public record Condition
{
    /// <summary>
    ///     Percentage change durability is altered
    /// </summary>
    [JsonPropertyName("conditionChance")]
    public double ConditionChance
    {
        get;
        set;
    }

    [JsonPropertyName("current")]
    public MinMax<double> Current
    {
        get;
        set;
    }

    [JsonPropertyName("max")]
    public MinMax<double> Max
    {
        get;
        set;
    }

    [JsonPropertyName("_name")]
    public string Name
    {
        get;
        set;
    }
}

public record RagfairBlacklist
{
    /// <summary>
    ///     Damaged ammo packs
    /// </summary>
    [JsonPropertyName("damagedAmmoPacks")]
    public bool DamagedAmmoPacks
    {
        get;
        set;
    }

    /// <summary>
    ///     Custom blacklist for item Tpls
    /// </summary>
    [JsonPropertyName("custom")]
    public HashSet<string> Custom
    {
        get;
        set;
    }

    /// <summary>
    ///     BSG blacklist a large number of items from flea, true = use blacklist
    /// </summary>
    [JsonPropertyName("enableBsgList")]
    public bool EnableBsgList
    {
        get;
        set;
    }

    /// <summary>
    ///     Should quest items be blacklisted from flea
    /// </summary>
    [JsonPropertyName("enableQuestList")]
    public bool EnableQuestList
    {
        get;
        set;
    }

    /// <summary>
    ///     Should trader items that are blacklisted by bsg be listed on flea
    /// </summary>
    [JsonPropertyName("traderItems")]
    public bool TraderItems
    {
        get;
        set;
    }

    /// <summary>
    ///     Maximum level an armor plate can be found in a flea-listed armor item
    /// </summary>
    [JsonPropertyName("armorPlate")]
    public ArmorPlateBlacklistSettings ArmorPlate
    {
        get;
        set;
    }

    /// <summary>
    ///     Should specific categories be blacklisted from the flea, true = use blacklist
    /// </summary>
    [JsonPropertyName("enableCustomItemCategoryList")]
    public bool EnableCustomItemCategoryList
    {
        get;
        set;
    }

    /// <summary>
    ///     Custom category blacklist for parent Ids
    /// </summary>
    [JsonPropertyName("customItemCategoryList")]
    public HashSet<string> CustomItemCategoryList
    {
        get;
        set;
    }
}

public record ArmorPlateBlacklistSettings
{
    /// <summary>
    ///     Max level of plates an armor can have without being removed
    /// </summary>
    [JsonPropertyName("maxProtectionLevel")]
    public int MaxProtectionLevel
    {
        get;
        set;
    }

    /// <summary>
    ///     Item slots to NOT remove from items on flea
    /// </summary>
    [JsonPropertyName("ignoreSlots")]
    public HashSet<string> IgnoreSlots
    {
        get;
        set;
    }
}

public record UnreasonableModPrices
{
    /// <summary>
    ///     Enable a system that adjusts very high ragfair prices to be below a max multiple of items the handbook values
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled
    {
        get;
        set;
    }

    /// <summary>
    ///     Multipler to start adjusting item values from, e.g. a value of 10 means any value over 10x the handbook price gets adjusted
    /// </summary>
    [JsonPropertyName("handbookPriceOverMultiplier")]
    public int HandbookPriceOverMultiplier
    {
        get;
        set;
    }

    /// <summary>
    ///     The new multiplier for items found using above property, e.g. a value of 4 means set items price to 4x handbook price
    /// </summary>
    [JsonPropertyName("newPriceHandbookMultiplier")]
    public int NewPriceHandbookMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("itemType")]
    public string ItemType
    {
        get;
        set;
    }
}

public record ArmorSettings
{
    /// <summary>
    ///     % chance / 100 that armor plates will be removed from an offer before listing
    /// </summary>
    [JsonPropertyName("removeRemovablePlateChance")]
    public int RemoveRemovablePlateChance
    {
        get;
        set;
    }

    /// <summary>
    ///     What slots are to be removed when removeRemovablePlateChance is true
    /// </summary>
    [JsonPropertyName("plateSlotIdToRemovePool")]
    public HashSet<string>? PlateSlotIdToRemovePool
    {
        get;
        set;
    }
}

public record TieredFlea
{
    [JsonPropertyName("enabled")]
    public bool Enabled
    {
        get;
        set;
    }

    /// <summary>
    ///     key: tpl, value: playerlevel
    /// </summary>
    [JsonPropertyName("unlocksTpl")]
    public Dictionary<string, int> UnlocksTpl
    {
        get;
        set;
    }

    /// <summary>
    ///     key: item type id, value: playerlevel
    /// </summary>
    [JsonPropertyName("unlocksType")]
    public Dictionary<string, int> UnlocksType
    {
        get;
        set;
    }

    [JsonPropertyName("ammoTplUnlocks")]
    public Dictionary<string, int>? AmmoTplUnlocks
    {
        get;
        set;
    }

    [JsonPropertyName("ammoTiersEnabled")]
    public bool AmmoTiersEnabled
    {
        get;
        set;
    }
}
