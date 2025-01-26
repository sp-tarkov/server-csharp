using Core.Models.Common;

namespace Core.Models.Spt.Config;

using System.Text.Json.Serialization;

public record RagfairConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-ragfair";

    /** How many seconds should pass before expired offers and processed + player offers checked if sold */
    [JsonPropertyName("runIntervalSeconds")]
    public int RunIntervalSeconds { get; set; }

    /** Default values used to hydrate `runIntervalSeconds` with */
    [JsonPropertyName("runIntervalValues")]
    public RunIntervalValues RunIntervalValues { get; set; }

    /** Player listing settings */
    [JsonPropertyName("sell")]
    public Sell Sell { get; set; }

    /** Trader ids + should their assorts be listed on flea */
    [JsonPropertyName("traders")]
    public Dictionary<string, bool> Traders { get; set; }

    [JsonPropertyName("dynamic")]
    public Dynamic Dynamic { get; set; }

    [JsonPropertyName("tieredFlea")]
    public TieredFlea TieredFlea { get; set; }
}

public record Sell
{
    /** Should a fee be deducted from player when listing an item for sale */
    [JsonPropertyName("fees")]
    public bool Fees { get; set; }

    /** Settings to control chances of offer being sold */
    [JsonPropertyName("chance")]
    public Chance Chance { get; set; }

    /** Settings to control how long it takes for a player offer to sell */
    [JsonPropertyName("time")]
    public MinMax Time { get; set; }

    /** Seconds from clicking remove to remove offer from market */
    [JsonPropertyName("expireSeconds")]
    public int ExpireSeconds { get; set; }
}

public record Chance
{
    /** Base chance percent to sell an item */
    [JsonPropertyName("base")]
    public int Base { get; set; }

    /** Value to multiply the sell chance by */
    [JsonPropertyName("sellMultiplier")]
    public double SellMultiplier { get; set; }

    /** Max possible sell chance % for a player listed offer */
    [JsonPropertyName("maxSellChancePercent")]
    public int MaxSellChancePercent { get; set; }

    /** Min possible sell chance % for a player listed offer */
    [JsonPropertyName("minSellChancePercent")]
    public int MinSellChancePercent { get; set; }
}

public record Dynamic
{
    [JsonPropertyName("purchasesAreFoundInRaid")]
    // Should a purchased dynamic offers items be flagged as found in raid
    public bool PurchasesAreFoundInRaid { get; set; }

    [JsonPropertyName("useTraderPriceForOffersIfHigher")]
    /** Use the highest trader price for an offer if its greater than the price in templates/prices.json */
    public bool UseTraderPriceForOffersIfHigher { get; set; }

    [JsonPropertyName("barter")]
    /** Barter offer specific settings */
    public BarterDetails Barter { get; set; }

    [JsonPropertyName("pack")]
    public PackDetails Pack { get; set; }

    [JsonPropertyName("offerAdjustment")]
    /** Dynamic offer price below handbook adjustment values */
    public OfferAdjustment OfferAdjustment { get; set; }

    [JsonPropertyName("expiredOfferThreshold")]
    /** How many offers should expire before an offer regeneration occurs */
    public int ExpiredOfferThreshold { get; set; }

    [JsonPropertyName("offerItemCount")]
    /** How many offers should be listed */
    public MinMax OfferItemCount { get; set; }

    [JsonPropertyName("priceRanges")]
    /** How much should the price of an offer vary by (percent 0.8 = 80%, 1.2 = 120%) */
    public PriceRanges PriceRanges { get; set; }

    [JsonPropertyName("showDefaultPresetsOnly")]
    /** Should default presets to listed only or should non-standard presets found in globals.json be listed too */
    public bool ShowDefaultPresetsOnly { get; set; }

    [JsonPropertyName("ignoreQualityPriceVarianceBlacklist")]
    /** Tpls that should not use the variable price system when their quality is < 100% (lower dura/uses = lower price) */
    public List<string> IgnoreQualityPriceVarianceBlacklist { get; set; }

    [JsonPropertyName("endTimeSeconds")]
    public MinMax EndTimeSeconds { get; set; }

    [JsonPropertyName("condition")]
    /** Settings to control the durability range of item items listed on flea */
    public Dictionary<string, Condition> Condition { get; set; }

    [JsonPropertyName("stackablePercent")]
    /** Size stackable items should be listed for in percent of max stack size */
    public MinMax StackablePercent { get; set; }

    [JsonPropertyName("nonStackableCount")]
    /** Items that cannot be stacked can have multiples sold in one offer, what range of values can be listed */
    public MinMax NonStackableCount { get; set; }

    [JsonPropertyName("rating")]
    /** Range of rating offers for items being listed */
    public MinMax Rating { get; set; }

    [JsonPropertyName("armor")]
    /** Armor specific flea settings */
    public ArmorSettings Armor { get; set; }

    [JsonPropertyName("itemPriceMultiplier")]
    /** A multipler to apply to individual tpls price just prior to item quality adjustment */
    public Dictionary<string, double>? ItemPriceMultiplier { get; set; }

    [JsonPropertyName("_currencies")]
    public string? CurrenciesDescription { get; set; }
    
    [JsonPropertyName("currencies")]
    /** Percentages to sell offers in each currency */
    public Dictionary<string, double> Currencies { get; set; }

    [JsonPropertyName("showAsSingleStack")]
    /** Item tpls that should be forced to sell as a single item */
    public List<string> ShowAsSingleStack { get; set; }

    [JsonPropertyName("removeSeasonalItemsWhenNotInEvent")]
    /** Should christmas/halloween items be removed from flea when not within the seasonal bounds */
    public bool RemoveSeasonalItemsWhenNotInEvent { get; set; }

    [JsonPropertyName("blacklist")]
    /** Flea blacklist settings */
    public RagfairBlacklist Blacklist { get; set; }

    [JsonPropertyName("unreasonableModPrices")]
    /** Dict of price limits keyed by item type */
    public Dictionary<string, UnreasonableModPrices> UnreasonableModPrices { get; set; }

    [JsonPropertyName("itemPriceOverrideRouble")]
    /** Custom rouble prices for items to override values from prices.json */
    public Dictionary<string, double> ItemPriceOverrideRouble { get; set; }
}

public record PriceRanges
{
    [JsonPropertyName("default")]
    public MinMax Default { get; set; }

    [JsonPropertyName("preset")]
    public MinMax Preset { get; set; }

    [JsonPropertyName("pack")]
    public MinMax Pack { get; set; }
}

public record BarterDetails
{
    /** Percentage change an offer is listed as a barter */
    [JsonPropertyName("chancePercent")]
    public double ChancePercent { get; set; }

    /** Min number of required items for a barter requirement */
    [JsonPropertyName("itemCountMin")]
    public int ItemCountMin { get; set; }

    /** Max number of required items for a barter requirement */
    [JsonPropertyName("itemCountMax")]
    public int ItemCountMax { get; set; }

    /** How much can the total price of requested items vary from the item offered */
    [JsonPropertyName("priceRangeVariancePercent")]
    public double PriceRangeVariancePercent { get; set; }

    /** Min rouble price for an offer to be considered for turning into a barter */
    [JsonPropertyName("minRoubleCostToBecomeBarter")]
    public double MinRoubleCostToBecomeBarter { get; set; }

    /** Should barter offers only single stack */
    [JsonPropertyName("makeSingleStackOnly")]
    public bool MakeSingleStackOnly { get; set; }

    /** Item Tpls to never be turned into a barter */
    [JsonPropertyName("itemTypeBlacklist")]
    public List<string> ItemTypeBlacklist { get; set; }
}

public record PackDetails
{
    /** Percentage change an offer is listed as a pack */
    [JsonPropertyName("chancePercent")]
    public double ChancePercent { get; set; }

    /** Min number of required items for a pack */
    [JsonPropertyName("itemCountMin")]
    public int ItemCountMin { get; set; }

    /** Max number of required items for a pack */
    [JsonPropertyName("itemCountMax")]
    public int ItemCountMax { get; set; }

    /** item types to allow being a pack */
    [JsonPropertyName("itemTypeWhitelist")]
    public List<string> ItemTypeWhitelist { get; set; }
}

public record OfferAdjustment
{
    /// <summary>
    /// Shuld offer price be adjusted when below handbook price
    /// </summary>
    [JsonPropertyName("adjustPriceWhenBelowHandbookPrice")]
    public bool AdjustPriceWhenBelowHandbookPrice { get; set; }

    /// <summary>
    /// How big a percentage difference does price need to vary from handbook to be considered for adjustment
    /// </summary>
    [JsonPropertyName("maxPriceDifferenceBelowHandbookPercent")]
    public double MaxPriceDifferenceBelowHandbookPercent { get; set; }

    /// <summary>
    /// How much to multiply the handbook price to get the new price
    /// </summary>
    [JsonPropertyName("handbookPriceMultipier")]
    public double HandbookPriceMultiplier { get; set; }

    /// <summary>
    /// What is the minimum rouble price to consider adjusting price of item
    /// </summary>
    [JsonPropertyName("priceThreshholdRub")]
    public double PriceThresholdRub { get; set; }
}

public record Condition
{
    /// <summary>
    /// Percentage change durability is altered
    /// </summary>
    [JsonPropertyName("conditionChance")]
    public double ConditionChance { get; set; }

    [JsonPropertyName("current")]
    public MinMax Current { get; set; }

    [JsonPropertyName("max")]
    public MinMax Max { get; set; }

    [JsonPropertyName("_name")]
    public string Name { get; set; }
}

public record RagfairBlacklist
{
    /// <summary>
    /// Damaged ammo packs
    /// </summary>
    [JsonPropertyName("damagedAmmoPacks")]
    public bool DamagedAmmoPacks { get; set; }

    /// <summary>
    /// Custom blacklist for item Tpls
    /// </summary>
    [JsonPropertyName("custom")]
    public List<string> Custom { get; set; }

    /// <summary>
    /// BSG blacklist a large number of items from flea, true = use blacklist
    /// </summary>
    [JsonPropertyName("enableBsgList")]
    public bool EnableBsgList { get; set; }

    /// <summary>
    /// Should quest items be blacklisted from flea
    /// </summary>
    [JsonPropertyName("enableQuestList")]
    public bool EnableQuestList { get; set; }

    /// <summary>
    /// Should trader items that are blacklisted by bsg be listed on flea
    /// </summary>
    [JsonPropertyName("traderItems")]
    public bool TraderItems { get; set; }

    /// <summary>
    /// Maximum level an armor plate can be found in a flea-listed armor item
    /// </summary>
    [JsonPropertyName("armorPlate")]
    public ArmorPlateBlacklistSettings ArmorPlate { get; set; }

    /// <summary>
    /// Should specific categories be blacklisted from the flea, true = use blacklist
    /// </summary>
    [JsonPropertyName("enableCustomItemCategoryList")]
    public bool EnableCustomItemCategoryList { get; set; }

    /// <summary>
    /// Custom category blacklist for parent Ids
    /// </summary>
    [JsonPropertyName("customItemCategoryList")]
    public List<string> CustomItemCategoryList { get; set; }
}

public record ArmorPlateBlacklistSettings
{
    /// <summary>
    /// Max level of plates an armor can have without being removed
    /// </summary>
    [JsonPropertyName("maxProtectionLevel")]
    public int MaxProtectionLevel { get; set; }

    /// <summary>
    /// Item slots to NOT remove from items on flea
    /// </summary>
    [JsonPropertyName("ignoreSlots")]
    public List<string> IgnoreSlots { get; set; }
}

public record UnreasonableModPrices
{
    /// <summary>
    /// Enable a system that adjusts very high ragfair prices to be below a max multiple of items the handbook values
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    /// <summary>
    /// Multipler to start adjusting item values from, e.g. a value of 10 means any value over 10x the handbook price gets adjusted
    /// </summary>
    [JsonPropertyName("handbookPriceOverMultiplier")]
    public int HandbookPriceOverMultiplier { get; set; }

    /// <summary>
    /// The new multiplier for items found using above property, e.g. a value of 4 means set items price to 4x handbook price
    /// </summary>
    [JsonPropertyName("newPriceHandbookMultiplier")]
    public int NewPriceHandbookMultiplier { get; set; }
    
    [JsonPropertyName("itemType")]
    public string ItemType { get; set; }
}

public record ArmorSettings
{
    /// <summary>
    /// % chance / 100 that armor plates will be removed from an offer before listing
    /// </summary>
    [JsonPropertyName("removeRemovablePlateChance")]
    public int RemoveRemovablePlateChance { get; set; }

    /// <summary>
    /// What slots are to be removed when removeRemovablePlateChance is true
    /// </summary>
    [JsonPropertyName("plateSlotIdToRemovePool")]
    public List<string> PlateSlotIdToRemovePool { get; set; }
}

public record TieredFlea
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    /// <summary>
    /// key: tpl, value: playerlevel
    /// </summary>
    [JsonPropertyName("unlocksTpl")]
    public Dictionary<string, int?> UnlocksTpl { get; set; }

    /// <summary>
    /// key: item type id, value: playerlevel
    /// </summary>
    [JsonPropertyName("unlocksType")]
    public Dictionary<string, int> UnlocksType { get; set; }

    [JsonPropertyName("ammoTplUnlocks")]
    public Dictionary<string, int?> AmmoTplUnlocks { get; set; }
    
    [JsonPropertyName("ammoTiersEnabled")]
    public bool AmmoTiersEnabled { get; set; }
}
