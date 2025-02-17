using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Spt.Services;

namespace Core.Models.Spt.Config;

public record TraderConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind
    {
        get;
        set;
    } = "spt-trader";

    [JsonPropertyName("updateTime")]
    public List<UpdateTime> UpdateTime
    {
        get;
        set;
    }

    [JsonPropertyName("updateTimeDefault")]
    public int UpdateTimeDefault
    {
        get;
        set;
    }

    [JsonPropertyName("purchasesAreFoundInRaid")]
    public bool PurchasesAreFoundInRaid
    {
        get;
        set;
    }

    /**
     * Should trader reset times be set based on server start time (false = bsg time - on the hour)
     */
    [JsonPropertyName("tradersResetFromServerStart")]
    public bool TradersResetFromServerStart
    {
        get;
        set;
    }

    [JsonPropertyName("traderPriceMultipler")]
    public double TraderPriceMultipler
    {
        get;
        set;
    }

    [JsonPropertyName("fence")]
    public FenceConfig Fence
    {
        get;
        set;
    }

    [JsonPropertyName("moddedTraders")]
    public ModdedTraders ModdedTraders
    {
        get;
        set;
    }
}

public record UpdateTime
{
    [JsonPropertyName("_name")]
    public string Name
    {
        get;
        set;
    }

    [JsonPropertyName("traderId")]
    public string TraderId
    {
        get;
        set;
    }

    /**
     * Seconds between trader resets
     */
    [JsonPropertyName("seconds")]
    public MinMax<int> Seconds
    {
        get;
        set;
    }
}

public record FenceConfig
{
    [JsonPropertyName("discountOptions")]
    public DiscountOptions DiscountOptions
    {
        get;
        set;
    }

    [JsonPropertyName("partialRefreshTimeSeconds")]
    public int PartialRefreshTimeSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("partialRefreshChangePercent")]
    public double PartialRefreshChangePercent
    {
        get;
        set;
    }

    [JsonPropertyName("assortSize")]
    public int AssortSize
    {
        get;
        set;
    }

    [JsonPropertyName("weaponPresetMinMax")]
    public MinMax<int> WeaponPresetMinMax
    {
        get;
        set;
    }

    [JsonPropertyName("equipmentPresetMinMax")]
    public MinMax<int> EquipmentPresetMinMax
    {
        get;
        set;
    }

    [JsonPropertyName("itemPriceMult")]
    public double ItemPriceMult
    {
        get;
        set;
    }

    [JsonPropertyName("presetPriceMult")]
    public double PresetPriceMult
    {
        get;
        set;
    }

    [JsonPropertyName("armorMaxDurabilityPercentMinMax")]
    public ItemDurabilityCurrentMax ArmorMaxDurabilityPercentMinMax
    {
        get;
        set;
    }

    [JsonPropertyName("weaponDurabilityPercentMinMax")]
    public ItemDurabilityCurrentMax WeaponDurabilityPercentMinMax
    {
        get;
        set;
    }

    /**
     * Keyed to plate protection level
     */
    [JsonPropertyName("chancePlateExistsInArmorPercent")]
    public Dictionary<string, double> ChancePlateExistsInArmorPercent
    {
        get;
        set;
    }

    /**
     * Key: item tpl
     */
    [JsonPropertyName("itemStackSizeOverrideMinMax")]
    public Dictionary<string, MinMax<int>?> ItemStackSizeOverrideMinMax
    {
        get;
        set;
    }

    [JsonPropertyName("itemTypeLimits")]
    public Dictionary<string, int> ItemTypeLimits
    {
        get;
        set;
    }

    /**
     * Prevent duplicate offers of items of specific categories by parentId
     */
    [JsonPropertyName("preventDuplicateOffersOfCategory")]
    public List<string> PreventDuplicateOffersOfCategory
    {
        get;
        set;
    }

    [JsonPropertyName("regenerateAssortsOnRefresh")]
    public bool RegenerateAssortsOnRefresh
    {
        get;
        set;
    }

    /**
     * Max rouble price before item is not listed on flea
     */
    [JsonPropertyName("itemCategoryRoublePriceLimit")]
    public Dictionary<string, double?> ItemCategoryRoublePriceLimit
    {
        get;
        set;
    }

    /**
     * Each slotid with % to be removed prior to listing on fence
     */
    [JsonPropertyName("presetSlotsToRemoveChancePercent")]
    public Dictionary<string, double?> PresetSlotsToRemoveChancePercent
    {
        get;
        set;
    }

    /**
     * Block seasonal items from appearing when season is inactive
     */
    [JsonPropertyName("blacklistSeasonalItems")]
    public bool BlacklistSeasonalItems
    {
        get;
        set;
    }

    /**
     * Max pen value allowed to be listed on flea - affects ammo + ammo boxes
     */
    [JsonPropertyName("ammoMaxPenLimit")]
    public double AmmoMaxPenLimit
    {
        get;
        set;
    }

    [JsonPropertyName("blacklist")]
    public HashSet<string> Blacklist
    {
        get;
        set;
    }

    [JsonPropertyName("coopExtractGift")]
    public CoopExtractReward CoopExtractGift
    {
        get;
        set;
    }

    [JsonPropertyName("btrDeliveryExpireHours")]
    public int BtrDeliveryExpireHours
    {
        get;
        set;
    }

    /**
     * Smallest value player rep with fence can fall to
     */
    [JsonPropertyName("playerRepMin")]
    public double PlayerRepMin
    {
        get;
        set;
    }

    /**
     * Highest value player rep with fence can climb to
     */
    [JsonPropertyName("playerRepMax")]
    public double PlayerRepMax
    {
        get;
        set;
    }
}

public record ItemDurabilityCurrentMax
{
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
}

public record CoopExtractReward : LootRequest
{
    [JsonPropertyName("sendGift")]
    public bool SendGift
    {
        get;
        set;
    }

    [JsonPropertyName("useRewardItemBlacklist")]
    public bool UseRewardItemBlacklist
    {
        get;
        set;
    }

    [JsonPropertyName("messageLocaleIds")]
    public List<string> MessageLocaleIds
    {
        get;
        set;
    }

    [JsonPropertyName("giftExpiryHours")]
    public int GiftExpiryHours
    {
        get;
        set;
    }
}

public record DiscountOptions
{
    [JsonPropertyName("assortSize")]
    public int AssortSize
    {
        get;
        set;
    }

    [JsonPropertyName("itemPriceMult")]
    public double ItemPriceMult
    {
        get;
        set;
    }

    [JsonPropertyName("presetPriceMult")]
    public double PresetPriceMult
    {
        get;
        set;
    }

    [JsonPropertyName("weaponPresetMinMax")]
    public MinMax<int> WeaponPresetMinMax
    {
        get;
        set;
    }

    [JsonPropertyName("equipmentPresetMinMax")]
    public MinMax<int> EquipmentPresetMinMax
    {
        get;
        set;
    }
}

/**
 * Custom trader data needed client side for things such as the clothing service
 */
public record ModdedTraders
{
    /**
     * Trader Ids to enable the clothing service for
     */
    [JsonPropertyName("clothingService")]
    public List<string> ClothingService
    {
        get;
        set;
    }
}
