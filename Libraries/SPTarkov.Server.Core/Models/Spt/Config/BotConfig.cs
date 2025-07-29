using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record BotConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind { get; set; } = "spt-bot";

    /// <summary>
    ///     How many variants of each bot should be generated on raid start
    /// </summary>
    [JsonPropertyName("presetBatch")]
    public required Dictionary<string, int> PresetBatch { get; set; }

    /// <summary>
    ///     What bot types should be classified as bosses
    /// </summary>
    [JsonPropertyName("bosses")]
    public required List<string> Bosses { get; set; }

    /// <summary>
    ///     Control weapon/armor durability min/max values for each bot type
    /// </summary>
    [JsonPropertyName("durability")]
    public required BotDurability Durability { get; set; }

    /// <summary>
    ///     Controls the percentage values of randomization item resources
    /// </summary>
    [JsonPropertyName("lootItemResourceRandomization")]
    public required Dictionary<string, RandomisedResourceDetails> LootItemResourceRandomization { get; set; }

    /// <summary>
    ///     Control how many items are allowed to spawn on a bot <br />
    ///     key: bottype, value: <br />
    ///     key: itemTpl: value: max item count>
    /// </summary>
    [JsonPropertyName("itemSpawnLimits")]
    public required Dictionary<string, Dictionary<MongoId, double>> ItemSpawnLimits { get; set; }

    /// <summary>
    ///     Blacklist/whitelist items on a bot
    /// </summary>
    [JsonPropertyName("equipment")]
    public required Dictionary<string, EquipmentFilters?> Equipment { get; set; }

    /// <summary>
    ///     Show a bots botType value after their name
    /// </summary>
    [JsonPropertyName("showTypeInNickname")]
    public bool ShowTypeInNickname { get; set; }

    /// <summary>
    ///     What ai brain should a normal scav use per map
    /// </summary>
    [JsonPropertyName("assaultBrainType")]
    public required Dictionary<string, Dictionary<string, int>> AssaultBrainType { get; set; }

    /// <summary>
    ///     What ai brain should a player scav use per map
    /// </summary>
    [JsonPropertyName("playerScavBrainType")]
    public required Dictionary<string, Dictionary<string, int>> PlayerScavBrainType { get; set; }

    /// <summary>
    ///     Max number of bots that can be spawned in a raid at any one time
    /// </summary>
    [JsonPropertyName("maxBotCap")]
    public required Dictionary<string, int> MaxBotCap { get; set; }

    /// <summary>
    ///     Chance scav has fake pscav name e.g. Scav name (player name)
    /// </summary>
    [JsonPropertyName("chanceAssaultScavHasPlayerScavName")]
    public int ChanceAssaultScavHasPlayerScavName { get; set; }

    /// <summary>
    ///     How many stacks of secret ammo should a bot have in its bot secure container
    /// </summary>
    [JsonPropertyName("secureContainerAmmoStackCount")]
    public int SecureContainerAmmoStackCount { get; set; }

    /// <summary>
    ///     Bot roles in this array will be given a dog tag on generation
    /// </summary>
    [JsonPropertyName("botRolesWithDogTags")]
    public required HashSet<string> BotRolesWithDogTags { get; set; }

    /// <summary>
    ///     Settings to control the items that get added into wallets on bots
    /// </summary>
    [JsonPropertyName("walletLoot")]
    public required WalletLootSettings WalletLoot { get; set; }

    /// <summary>
    ///     Currency weights, Keyed by botrole / currency
    /// </summary>
    [JsonPropertyName("currencyStackSize")]
    public required Dictionary<string, Dictionary<string, Dictionary<string, double>>> CurrencyStackSize { get; set; }

    /// <summary>
    ///     Tpls for low profile gas blocks
    /// </summary>
    [JsonPropertyName("lowProfileGasBlockTpls")]
    public required HashSet<string> LowProfileGasBlockTpls { get; set; }

    /// <summary>
    ///     What bottypes should be excluded from having loot generated on them (backpack/pocket/vest) does not disable food/drink/special/
    /// </summary>
    [JsonPropertyName("disableLootOnBotTypes")]
    public required HashSet<string> DisableLootOnBotTypes { get; set; }

    /// <summary>
    ///     Max length a bots name can be
    /// </summary>
    [JsonPropertyName("botNameLengthLimit")]
    public int BotNameLengthLimit { get; set; }

    /// <summary>
    ///     Bot roles that must have a unique name when generated vs other bots in raid
    /// </summary>
    [JsonPropertyName("botRolesThatMustHaveUniqueName")]
    public required HashSet<string> BotRolesThatMustHaveUniqueName { get; set; }

    /// <summary>
    ///     Bot roles that must have a unique name when generated vs other bots in raid
    /// </summary>
    [JsonPropertyName("weeklyBoss")]
    public required WeeklyBossSettings WeeklyBoss { get; set; }
}

public record WeeklyBossSettings
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    /// <summary>
    /// Should a weekly boss be picked to 100% spawn
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    /// <summary>
    /// Bosses that can be picked
    /// </summary>
    [JsonPropertyName("bossPool")]
    public required List<WildSpawnType> BossPool { get; set; }

    /// <summary>
    /// Day of week boss choice resets
    /// </summary>
    [JsonPropertyName("resetDay")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DayOfWeek ResetDay { get; set; }
}

public record WalletLootSettings
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    /// <summary>
    ///     Chance wallets have loot in them
    /// </summary>
    [JsonPropertyName("chancePercent")]
    public float ChancePercent { get; set; }

    [JsonPropertyName("itemCount")]
    public required MinMax<int> ItemCount { get; set; }

    [JsonPropertyName("stackSizeWeight")]
    public required Dictionary<string, double> StackSizeWeight { get; set; }

    [JsonPropertyName("currencyWeight")]
    public required Dictionary<MongoId, double> CurrencyWeight { get; set; }

    /// <summary>
    ///     What wallets will have money in them
    /// </summary>
    [JsonPropertyName("walletTplPool")]
    public required HashSet<MongoId> WalletTplPool { get; set; }
}

public record EquipmentFilters
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    /// <summary>
    ///     Limits for mod types per weapon .e.g. scopes
    /// </summary>
    [JsonPropertyName("weaponModLimits")]
    public ModLimits? WeaponModLimits { get; set; }

    /// <summary>
    ///     Whitelist for weapon sight types allowed per gun
    /// </summary>
    [JsonPropertyName("weaponSightWhitelist")]
    public Dictionary<MongoId, HashSet<MongoId>>? WeaponSightWhitelist { get; set; }

    [JsonPropertyName("forceOnlyArmoredRigWhenNoArmor")]
    public bool? ForceOnlyArmoredRigWhenNoArmor { get; set; }

    /// <summary>
    ///     Should plates be filtered by level
    /// </summary>
    [JsonPropertyName("filterPlatesByLevel")]
    public bool? FilterPlatesByLevel { get; set; }

    /// <summary>
    ///     What additional slot ids should be seen as required when choosing a mod to add to a weapon
    /// </summary>
    [JsonPropertyName("weaponSlotIdsToMakeRequired")]
    public HashSet<string>? WeaponSlotIdsToMakeRequired { get; set; }

    /// <summary>
    ///     Adjust weighting/chances of items on bot by level of bot
    /// </summary>
    [JsonPropertyName("randomisation")]
    public List<RandomisationDetails>? Randomisation { get; set; }

    /// <summary>
    ///     Blacklist equipment by level of bot
    /// </summary>
    [JsonPropertyName("blacklist")]
    public List<EquipmentFilterDetails>? Blacklist { get; set; }

    /// <summary>
    ///     Whitelist equipment by level of bot
    /// </summary>
    [JsonPropertyName("whitelist")]
    public List<EquipmentFilterDetails>? Whitelist { get; set; }

    /// <summary>
    ///     Adjust equipment/ammo
    /// </summary>
    [JsonPropertyName("weightingAdjustmentsByBotLevel")]
    public List<WeightingAdjustmentDetails>? WeightingAdjustmentsByBotLevel { get; set; }

    /// <summary>
    ///     Should the stock mod be forced to spawn on bot
    /// </summary>
    [JsonPropertyName("forceStock")]
    public bool? ForceStock { get; set; }

    [JsonPropertyName("armorPlateWeighting")]
    public List<ArmorPlateWeights>? ArmorPlateWeighting { get; set; }

    /// <summary>
    ///     Chance face shield is down/active
    /// </summary>
    [JsonPropertyName("faceShieldIsActiveChancePercent")]
    public double? FaceShieldIsActiveChancePercent { get; set; }

    /// <summary>
    ///     Chance gun flashlight is active during the day
    /// </summary>
    [JsonPropertyName("lightIsActiveDayChancePercent")]
    public double? LightIsActiveDayChancePercent { get; set; }

    /// <summary>
    ///     Chance gun flashlight is active during the night
    /// </summary>
    [JsonPropertyName("lightIsActiveNightChancePercent")]
    public double? LightIsActiveNightChancePercent { get; set; }

    /// <summary>
    ///     Chance gun laser is active during the day
    /// </summary>
    [JsonPropertyName("laserIsActiveChancePercent")]
    public double? LaserIsActiveChancePercent { get; set; }

    /// <summary>
    ///     Chance NODS are down/active during the day
    /// </summary>
    [JsonPropertyName("nvgIsActiveChanceDayPercent")]
    public double? NvgIsActiveChanceDayPercent { get; set; }

    /// <summary>
    ///     Chance NODS are down/active during the night
    /// </summary>
    [JsonPropertyName("nvgIsActiveChanceNightPercent")]
    public double? NvgIsActiveChanceNightPercent { get; set; }

    [JsonPropertyName("forceRigWhenNoVest")]
    public bool? ForceRigWhenNoVest { get; set; }
}

public record ModLimits
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    /// <summary>
    ///     How many scopes are allowed on a weapon - hard coded to work with OPTIC_SCOPE, ASSAULT_SCOPE, COLLIMATOR, COMPACT_COLLIMATOR
    /// </summary>
    [JsonPropertyName("scopeLimit")]
    public int? ScopeLimit { get; set; }

    /// <summary>
    ///     How many lasers or lights are allowed on a weapon - hard coded to work with TACTICAL_COMBO, and FLASHLIGHT
    /// </summary>
    [JsonPropertyName("lightLaserLimit")]
    public int? LightLaserLimit { get; set; }
}

public record RandomisationDetails
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    /// <summary>
    ///     Between what levels do these randomisation setting apply to
    /// </summary>
    [JsonPropertyName("levelRange")]
    public required MinMax<int> LevelRange { get; set; }

    [JsonPropertyName("generation")]
    public Dictionary<string, GenerationData>? Generation { get; set; }

    /// <summary>
    ///     Mod slots that should be fully randomised -ignores mods from bottype.json and instead creates a pool using items.json
    /// </summary>
    [JsonPropertyName("randomisedWeaponModSlots")]
    public HashSet<string>? RandomisedWeaponModSlots { get; set; }

    /// <summary>
    ///     Armor slots that should be randomised e.g. 'Headwear, Armband'
    /// </summary>
    [JsonPropertyName("randomisedArmorSlots")]
    public HashSet<string>? RandomisedArmorSlots { get; set; }

    /// <summary>
    ///     Equipment chances
    /// </summary>
    [JsonPropertyName("equipment")]
    public Dictionary<string, double>? Equipment { get; set; }

    /// <summary>
    ///     Weapon mod chances
    /// </summary>
    [JsonPropertyName("weaponMods")]
    public Dictionary<string, double>? WeaponMods { get; set; }

    /// <summary>
    ///     Equipment mod chances
    /// </summary>
    [JsonPropertyName("equipmentMods")]
    public Dictionary<string, double>? EquipmentMods { get; set; }

    [JsonPropertyName("nighttimeChanges")]
    public NighttimeChanges? NighttimeChanges { get; set; }

    /// <summary>
    ///     Key = weapon tpl, value = min size of magazine allowed
    /// </summary>
    [JsonPropertyName("minimumMagazineSize")]
    public Dictionary<string, double>? MinimumMagazineSize { get; set; }
}

public record NighttimeChanges
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    /// <summary>
    ///     Applies changes to values stored in equipmentMods
    /// </summary>
    [JsonPropertyName("equipmentModsModifiers")]
    public required Dictionary<string, float> EquipmentModsModifiers { get; set; }
}

public record EquipmentFilterDetails
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    /// <summary>
    ///     Between what levels do these equipment filter setting apply to
    /// </summary>
    [JsonPropertyName("levelRange")]
    public MinMax<int> LevelRange { get; set; }

    /// <summary>
    ///     Key: mod slot name e.g. mod_magazine, value: item tpls
    /// </summary>
    [JsonPropertyName("equipment")]
    public Dictionary<string, HashSet<MongoId>>? Equipment { get; set; }

    /// <summary>
    ///     Key: cartridge type e.g. Caliber23x75, value: item tpls
    /// </summary>
    [JsonPropertyName("cartridge")]
    public Dictionary<string, HashSet<MongoId>>? Cartridge { get; set; }
}

public record WeightingAdjustmentDetails
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    /// <summary>
    ///     Between what levels do these weight settings apply to
    /// </summary>
    [JsonPropertyName("levelRange")]
    public required MinMax<int> LevelRange { get; set; }

    /// <summary>
    ///     Key: ammo type e.g. Caliber556x45NATO, value: item tpl + weight
    /// </summary>
    [JsonPropertyName("ammo")]
    public AdjustmentDetails? Ammo { get; set; }

    /// <summary>
    ///     Key: equipment slot e.g. TacticalVest, value: item tpl + weight
    /// </summary>
    [JsonPropertyName("equipment")]
    public AdjustmentDetails? Equipment { get; set; }

    /// <summary>
    ///     Key: clothing slot e.g. feet, value: item tpl + weight
    /// </summary>
    [JsonPropertyName("clothing")]
    public AdjustmentDetails? Clothing { get; set; }
}

public record AdjustmentDetails
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    [JsonPropertyName("add")]
    public Dictionary<string, Dictionary<MongoId, float>>? Add { get; set; }

    [JsonPropertyName("edit")]
    public Dictionary<string, Dictionary<MongoId, float>>? Edit { get; set; }
}

public class ArmorPlateWeights
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    [JsonPropertyName("levelRange")]
    public required MinMax<int> LevelRange { get; set; }

    [JsonPropertyName("values")]
    public required Dictionary<string, Dictionary<string, double>> Values { get; set; }
}

public record RandomisedResourceDetails
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    [JsonPropertyName("food")]
    public RandomisedResourceValues? Food { get; set; }

    [JsonPropertyName("meds")]
    public RandomisedResourceValues? Meds { get; set; }
}

public record RandomisedResourceValues
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    /// <summary>
    ///     Minimum percent of item to randomized between min and max resource
    /// </summary>
    [JsonPropertyName("resourcePercent")]
    public float ResourcePercent { get; set; }

    /// <summary>
    ///     Chance for randomization to not occur
    /// </summary>
    [JsonPropertyName("chanceMaxResourcePercent")]
    public float ChanceMaxResourcePercent { get; set; }
}
