using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record RepairConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind
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

    /// <summary>
    ///     INT gain multiplier per repaired item type
    /// </summary>
    [JsonPropertyName("repairKitIntellectGainMultiplier")]
    public required IntellectGainValues RepairKitIntellectGainMultiplier
    {
        get;
        set;
    }

    /// <summary>
    ///     How much INT can be given to player per repair action
    /// </summary>
    [JsonPropertyName("maxIntellectGainPerRepair")]
    public required MaxIntellectGainValues MaxIntellectGainPerRepair
    {
        get;
        set;
    }

    [JsonPropertyName("weaponTreatment")]
    public required WeaponTreatmentRepairValues WeaponTreatment
    {
        get;
        set;
    }

    [JsonPropertyName("repairKit")]
    public required RepairKit RepairKit
    {
        get;
        set;
    }
}

public record IntellectGainValues
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

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
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

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
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    /// <summary>
    ///     The chance to gain more weapon maintenance skill
    /// </summary>
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

    /// <summary>
    ///     The chance to gain less weapon maintenance skill
    /// </summary>
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

    /// <summary>
    ///     The multiplier used for calculating weapon maintenance XP
    /// </summary>
    [JsonPropertyName("pointGainMultiplier")]
    public double PointGainMultiplier
    {
        get;
        set;
    }
}

public record RepairKit
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("armor")]
    public required BonusSettings Armor
    {
        get;
        set;
    }

    [JsonPropertyName("weapon")]
    public required BonusSettings Weapon
    {
        get;
        set;
    }

    [JsonPropertyName("vest")]
    public required BonusSettings Vest
    {
        get;
        set;
    }

    [JsonPropertyName("headwear")]
    public required BonusSettings Headwear
    {
        get;
        set;
    }
}

public record BonusSettings
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("rarityWeight")]
    public required Dictionary<string, double> RarityWeight
    {
        get;
        set;
    }

    [JsonPropertyName("bonusTypeWeight")]
    public required Dictionary<string, double> BonusTypeWeight
    {
        get;
        set;
    }

    [JsonPropertyName("Common")]
    public required Dictionary<string, BonusValues> Common
    {
        get;
        set;
    }

    [JsonPropertyName("Rare")]
    public required Dictionary<string, BonusValues> Rare
    {
        get;
        set;
    }
}

public record BonusValues
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("valuesMinMax")]
    public required MinMax<double> ValuesMinMax
    {
        get;
        set;
    }

    /// <summary>
    ///     What dura is buff active between (min max of current max)
    /// </summary>
    [JsonPropertyName("activeDurabilityPercentMinMax")]
    public required MinMax<int> ActiveDurabilityPercentMinMax
    {
        get;
        set;
    }
}
