using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Spt.Config;

public record RepairConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind
    {
        get;
        set;
    } = "spt-repair";

    [JsonPropertyName("priceMultiplier")]
    public double PriceMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("applyRandomizeDurabilityLoss")]
    public bool ApplyRandomizeDurabilityLoss
    {
        get;
        set;
    }

    [JsonPropertyName("weaponSkillRepairGain")]
    public double WeaponSkillRepairGain
    {
        get;
        set;
    }

    [JsonPropertyName("armorKitSkillPointGainPerRepairPointMultiplier")]
    public double ArmorKitSkillPointGainPerRepairPointMultiplier
    {
        get;
        set;
    }

    /**
     * INT gain multiplier per repaired item type
     */
    [JsonPropertyName("repairKitIntellectGainMultiplier")]
    public IntellectGainValues RepairKitIntellectGainMultiplier
    {
        get;
        set;
    }

    // ** How much INT can be given to player per repair action */
    [JsonPropertyName("maxIntellectGainPerRepair")]
    public MaxIntellectGainValues MaxIntellectGainPerRepair
    {
        get;
        set;
    }

    [JsonPropertyName("weaponTreatment")]
    public WeaponTreatmentRepairValues WeaponTreatment
    {
        get;
        set;
    }

    [JsonPropertyName("repairKit")]
    public RepairKit RepairKit
    {
        get;
        set;
    }
}

public record IntellectGainValues
{
    [JsonPropertyName("weapon")]
    public double Weapon
    {
        get;
        set;
    }

    [JsonPropertyName("armor")]
    public double Armor
    {
        get;
        set;
    }
}

public record MaxIntellectGainValues
{
    [JsonPropertyName("kit")]
    public double Kit
    {
        get;
        set;
    }

    [JsonPropertyName("trader")]
    public double Trader
    {
        get;
        set;
    }
}

public record WeaponTreatmentRepairValues
{
    /**
     * The chance to gain more weapon maintenance skill
     */
    [JsonPropertyName("critSuccessChance")]
    public double CritSuccessChance
    {
        get;
        set;
    }

    [JsonPropertyName("critSuccessAmount")]
    public double CritSuccessAmount
    {
        get;
        set;
    }

    /**
     * The chance to gain less weapon maintenance skill
     */
    [JsonPropertyName("critFailureChance")]
    public double CritFailureChance
    {
        get;
        set;
    }

    [JsonPropertyName("critFailureAmount")]
    public double CritFailureAmount
    {
        get;
        set;
    }

    /**
     * The multiplier used for calculating weapon maintenance XP
     */
    [JsonPropertyName("pointGainMultiplier")]
    public double PointGainMultiplier
    {
        get;
        set;
    }
}

public record RepairKit
{
    [JsonPropertyName("armor")]
    public BonusSettings Armor
    {
        get;
        set;
    }

    [JsonPropertyName("weapon")]
    public BonusSettings Weapon
    {
        get;
        set;
    }

    [JsonPropertyName("vest")]
    public BonusSettings Vest
    {
        get;
        set;
    }

    [JsonPropertyName("headwear")]
    public BonusSettings Headwear
    {
        get;
        set;
    }
}

public record BonusSettings
{
    [JsonPropertyName("rarityWeight")]
    public Dictionary<string, double> RarityWeight
    {
        get;
        set;
    }

    [JsonPropertyName("bonusTypeWeight")]
    public Dictionary<string, double> BonusTypeWeight
    {
        get;
        set;
    }

    [JsonPropertyName("Common")]
    public Dictionary<string, BonusValues> Common
    {
        get;
        set;
    }

    [JsonPropertyName("Rare")]
    public Dictionary<string, BonusValues> Rare
    {
        get;
        set;
    }
}

public record BonusValues
{
    [JsonPropertyName("valuesMinMax")]
    public MinMax<double> ValuesMinMax
    {
        get;
        set;
    }

    /**
     * What dura is buff active between (min max of current max)
     */
    [JsonPropertyName("activeDurabilityPercentMinMax")]
    public MinMax<int> ActiveDurabilityPercentMinMax
    {
        get;
        set;
    }
}
