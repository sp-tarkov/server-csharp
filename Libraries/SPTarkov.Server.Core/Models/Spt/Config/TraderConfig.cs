using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Services;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record TraderConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind
    {
        get;
        set;
    } = "spt-trader";

    [JsonPropertyName("updateTime")]
    public List<UpdateTime> UpdateTime
    {
        get;
        set;
    } = [];

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

    /// <summary>
    ///     Should trader reset times be set based on server start time (false = bsg time - on the hour)
    /// </summary>
    [JsonPropertyName("tradersResetFromServerStart")]
    public bool TradersResetFromServerStart
    {
        get;
        set;
    }

    [JsonPropertyName("traderPriceMultipler")]
    public double TraderPriceMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("fence")]
    public required FenceConfig Fence
    {
        get;
        set;
    }

    [JsonPropertyName("moddedTraders")]
    public required ModdedTraders ModdedTraders
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
    } = string.Empty;

    [JsonPropertyName("traderId")]
    public string TraderId
    {
        get;
        set;
    } = string.Empty;

    /// <summary>
    ///     Seconds between trader resets
    /// </summary>
    [JsonPropertyName("seconds")]
    public required MinMax<int> Seconds
    {
        get;
        set;
    }
}

public record FenceConfig
{
    [JsonPropertyName("discountOptions")]
    public required DiscountOptions DiscountOptions
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
    public required MinMax<int> WeaponPresetMinMax
    {
        get;
        set;
    }

    [JsonPropertyName("equipmentPresetMinMax")]
    public required MinMax<int> EquipmentPresetMinMax
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
    public required ItemDurabilityCurrentMax ArmorMaxDurabilityPercentMinMax
    {
        get;
        set;
    }

    [JsonPropertyName("weaponDurabilityPercentMinMax")]
    public required ItemDurabilityCurrentMax WeaponDurabilityPercentMinMax
    {
        get;
        set;
    }

    /// <summary>
    ///     Keyed to plate protection level
    /// </summary>
    [JsonPropertyName("chancePlateExistsInArmorPercent")]
    public required Dictionary<string, double> ChancePlateExistsInArmorPercent
    {
        get;
        set;
    }

    /// <summary>
    ///     Key: item tpl
    /// </summary>
    [JsonPropertyName("itemStackSizeOverrideMinMax")]
    public required Dictionary<string, MinMax<int>?> ItemStackSizeOverrideMinMax
    {
        get;
        set;
    }

    [JsonPropertyName("itemTypeLimits")]
    public required Dictionary<string, int> ItemTypeLimits
    {
        get;
        set;
    }

    /// <summary>
    ///     Prevent duplicate offers of items of specific categories by parentId
    /// </summary>
    [JsonPropertyName("preventDuplicateOffersOfCategory")]
    public required List<string> PreventDuplicateOffersOfCategory
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

    /// <summary>
    ///     Max rouble price before item is not listed on flea
    /// </summary>
    [JsonPropertyName("itemCategoryRoublePriceLimit")]
    public required Dictionary<string, double?> ItemCategoryRoublePriceLimit
    {
        get;
        set;
    }

    /// <summary>
    ///     Each slotid with % to be removed prior to listing on fence
    /// </summary>
    [JsonPropertyName("presetSlotsToRemoveChancePercent")]
    public required Dictionary<string, double?> PresetSlotsToRemoveChancePercent
    {
        get;
        set;
    }

    /// <summary>
    ///     Block seasonal items from appearing when season is inactive
    /// </summary>
    [JsonPropertyName("blacklistSeasonalItems")]
    public bool BlacklistSeasonalItems
    {
        get;
        set;
    }

    /// <summary>
    ///     Max pen value allowed to be listed on flea - affects ammo + ammo boxes
    /// </summary>
    [JsonPropertyName("ammoMaxPenLimit")]
    public double AmmoMaxPenLimit
    {
        get;
        set;
    }

    [JsonPropertyName("blacklist")]
    public required HashSet<string> Blacklist
    {
        get;
        set;
    }

    [JsonPropertyName("coopExtractGift")]
    public required CoopExtractReward CoopExtractGift
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

    /// <summary>
    ///     Smallest value player rep with fence can fall to
    /// </summary>
    [JsonPropertyName("playerRepMin")]
    public double PlayerRepMin
    {
        get;
        set;
    }

    /// <summary>
    ///     Highest value player rep with fence can climb to
    /// </summary>
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
    public required MinMax<double> Current
    {
        get;
        set;
    }

    [JsonPropertyName("max")]
    public required MinMax<double> Max
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
    public new bool UseRewardItemBlacklist
    {
        get;
        set;
    }

    [JsonPropertyName("messageLocaleIds")]
    public required List<string> MessageLocaleIds
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
    public required MinMax<int> WeaponPresetMinMax
    {
        get;
        set;
    }

    [JsonPropertyName("equipmentPresetMinMax")]
    public required MinMax<int> EquipmentPresetMinMax
    {
        get;
        set;
    }
}

/// <summary>
///     Custom trader data needed client side for things such as the clothing service
/// </summary>
public record ModdedTraders
{
    /// <summary>
    ///     Trader Ids to enable the clothing service for
    /// </summary>
    [JsonPropertyName("clothingService")]
    public List<string> ClothingService
    {
        get;
        set;
    } = [];
}
