using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;

namespace Core.Models.Spt.Config;

public record BotConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-bot";

    /** How many variants of each bot should be generated on raid start */
    [JsonPropertyName("presetBatch")]
    public Dictionary<string, int>? PresetBatch { get; set; }

    /** Bot roles that should not have PMC types (pmcBEAR/pmcUSEC) added as enemies to */
    [JsonPropertyName("botsToNotAddPMCsAsEnemiesTo")]
    public List<string> BotsToNotAddPMCsAsEnemiesTo { get; set; }

    /** What bot types should be classified as bosses */
    [JsonPropertyName("bosses")]
    public List<string> Bosses { get; set; }

    /** Control weapon/armor durability min/max values for each bot type */
    [JsonPropertyName("durability")]
    public BotDurability Durability { get; set; }

    /** Controls the percentage values of randomization item resources */
    [JsonPropertyName("lootItemResourceRandomization")]
    public Dictionary<string, RandomisedResourceDetails> LootItemResourceRandomization { get; set; }

    /** Control what bots are added to a bots revenge list key: bottype, value: bottypes to revenge on seeing their death */
    [JsonPropertyName("revenge")]
    public Dictionary<string, List<string>> Revenge { get; set; }

    /** Control how many items are allowed to spawn on a bot
     * key: bottype, value: <key: itemTpl: value: max item count> */
    [JsonPropertyName("itemSpawnLimits")]
    public Dictionary<string, Dictionary<string, double>> ItemSpawnLimits { get; set; }

    /** Blacklist/whitelist items on a bot */
    [JsonPropertyName("equipment")]
    public Dictionary<string, EquipmentFilters?> Equipment { get; set; }

    /** Show a bots botType value after their name */
    [JsonPropertyName("showTypeInNickname")]
    public bool ShowTypeInNickname { get; set; }

    /** What ai brain should a normal scav use per map */
    [JsonPropertyName("assaultBrainType")]
    public Dictionary<string, Dictionary<string, int>> AssaultBrainType { get; set; }

    /** What ai brain should a player scav use per map */
    [JsonPropertyName("playerScavBrainType")]
    public Dictionary<string, Dictionary<string, int>> PlayerScavBrainType { get; set; }

    /** Max number of bots that can be spawned in a raid at any one time */
    [JsonPropertyName("maxBotCap")]
    public Dictionary<string, int> MaxBotCap { get; set; }

    /** Chance scav has fake pscav name e.g. Scav name (player name) */
    [JsonPropertyName("chanceAssaultScavHasPlayerScavName")]
    public int ChanceAssaultScavHasPlayerScavName { get; set; }

    /** How many stacks of secret ammo should a bot have in its bot secure container */
    [JsonPropertyName("secureContainerAmmoStackCount")]
    public int SecureContainerAmmoStackCount { get; set; }

    /** Bot roles in this array will be given a dog tag on generation */
    [JsonPropertyName("botRolesWithDogTags")]
    public List<string> BotRolesWithDogTags { get; set; }

    /** Settings to control the items that get added into wallets on bots */
    [JsonPropertyName("walletLoot")]
    public WalletLootSettings WalletLoot { get; set; }

    /** Currency weights, Keyed by botrole / currency */
    [JsonPropertyName("currencyStackSize")]
    public Dictionary<string, Dictionary<string, Dictionary<string, double>>> CurrencyStackSize { get; set; }

    /** Tpls for low profile gas blocks */
    [JsonPropertyName("lowProfileGasBlockTpls")]
    public List<string> LowProfileGasBlockTpls { get; set; }

    /** What bottypes should be excluded from having loot generated on them (backpack/pocket/vest) does not disable food/drink/special/ */
    [JsonPropertyName("disableLootOnBotTypes")]
    public List<string> DisableLootOnBotTypes { get; set; }

    [JsonPropertyName("assaultToBossConversion")]
    public AssaultToBossConversion AssaultToBossConversion { get; set; }

    /** Max length a bots name can be */
    [JsonPropertyName("botNameLengthLimit")]
    public int BotNameLengthLimit { get; set; }

    /** Bot roles that must have a unique name when generated vs other bots in raid */
    [JsonPropertyName("botRolesThatMustHaveUniqueName")]
    public List<string> BotRolesThatMustHaveUniqueName { get; set; }
}

public record AssaultToBossConversion
{
    [JsonPropertyName("bossConvertEnabled")]
    public bool BossConvertEnabled { get; set; }

    [JsonPropertyName("bossesToConvertToWeights")]
    public Dictionary<string, double> BossesToConvertToWeights { get; set; }

    [JsonPropertyName("bossConvertMinMax")]
    public Dictionary<string, MinMax> BossConvertMinMax { get; set; }
}

/** Number of bots to generate and store in cache on raid start per bot type */
public record PresetBatch
{
    [JsonPropertyName("assault")]
    public int Assault { get; set; }

    [JsonPropertyName("bossBully")]
    public int BossBully { get; set; }

    [JsonPropertyName("bossGluhar")]
    public int BossGluhar { get; set; }

    [JsonPropertyName("bossKilla")]
    public int BossKilla { get; set; }

    [JsonPropertyName("bossKojaniy")]
    public int BossKojaniy { get; set; }

    [JsonPropertyName("bossSanitar")]
    public int BossSanitar { get; set; }

    [JsonPropertyName("bossTagilla")]
    public int BossTagilla { get; set; }

    [JsonPropertyName("bossKnight")]
    public int BossKnight { get; set; }

    [JsonPropertyName("bossZryachiy")]
    public int BossZryachiy { get; set; }

    [JsonPropertyName("bossKolontay")]
    public int BossKolontay { get; set; }

    [JsonPropertyName("bossPartisan")]
    public int BossPartisan { get; set; }

    [JsonPropertyName("bossTest")]
    public int BossTest { get; set; }

    [JsonPropertyName("cursedAssault")]
    public int CursedAssault { get; set; }

    [JsonPropertyName("followerBully")]
    public int FollowerBully { get; set; }

    [JsonPropertyName("followerGluharAssault")]
    public int FollowerGluharAssault { get; set; }

    [JsonPropertyName("followerGluharScout")]
    public int FollowerGluharScout { get; set; }

    [JsonPropertyName("followerGluharSecurity")]
    public int FollowerGluharSecurity { get; set; }

    [JsonPropertyName("followerGluharSnipe")]
    public int FollowerGluharSnipe { get; set; }

    [JsonPropertyName("followerKojaniy")]
    public int FollowerKojaniy { get; set; }

    [JsonPropertyName("followerSanitar")]
    public int FollowerSanitar { get; set; }

    [JsonPropertyName("followerTagilla")]
    public int FollowerTagilla { get; set; }

    [JsonPropertyName("followerBirdEye")]
    public int FollowerBirdEye { get; set; }

    [JsonPropertyName("followerBigPipe")]
    public int FollowerBigPipe { get; set; }

    [JsonPropertyName("followerTest")]
    public int FollowerTest { get; set; }

    [JsonPropertyName("followerBoar")]
    public int FollowerBoar { get; set; }

    [JsonPropertyName("followerBoarClose1")]
    public int FollowerBoarClose1 { get; set; }

    [JsonPropertyName("followerBoarClose2")]
    public int FollowerBoarClose2 { get; set; }

    [JsonPropertyName("followerZryachiy")]
    public int FollowerZryachiy { get; set; }

    [JsonPropertyName("followerKolontayAssault")]
    public int FollowerKolontayAssault { get; set; }

    [JsonPropertyName("followerKolontaySecurity")]
    public int FollowerKolontaySecurity { get; set; }

    [JsonPropertyName("marksman")]
    public int Marksman { get; set; }

    [JsonPropertyName("pmcBot")]
    public int PmcBot { get; set; }

    [JsonPropertyName("sectantPriest")]
    public int SectantPriest { get; set; }

    [JsonPropertyName("sectantWarrior")]
    public int SectantWarrior { get; set; }

    [JsonPropertyName("gifter")]
    public int Gifter { get; set; }

    [JsonPropertyName("test")]
    public int Test { get; set; }

    [JsonPropertyName("exUsec")]
    public int ExUsec { get; set; }

    [JsonPropertyName("arenaFighterEvent")]
    public int ArenaFighterEvent { get; set; }

    [JsonPropertyName("arenaFighter")]
    public int ArenaFighter { get; set; }

    [JsonPropertyName("crazyAssaultEvent")]
    public int CrazyAssaultEvent { get; set; }

    [JsonPropertyName("bossBoar")]
    public int BossBoar { get; set; }

    [JsonPropertyName("bossBoarSniper")]
    public int BossBoarSniper { get; set; }

    [JsonPropertyName("pmcUSEC")]
    public int PmcUSEC { get; set; }

    [JsonPropertyName("pmcBEAR")]
    public int PmcBEAR { get; set; }

    [JsonPropertyName("shooterBTR")]
    public int ShooterBTR { get; set; }
    
    [JsonExtensionData]
    public IDictionary<string, object> AdditionalData { get; set; }
}

public record WalletLootSettings
{
    /// <summary>
    /// Chance wallets have loot in them
    /// </summary>
    [JsonPropertyName("chancePercent")]
    public float ChancePercent { get; set; }

    [JsonPropertyName("itemCount")]
    public MinMax ItemCount { get; set; }

    [JsonPropertyName("stackSizeWeight")]
    public Dictionary<string, double> StackSizeWeight { get; set; }

    [JsonPropertyName("currencyWeight")]
    public Dictionary<string, double> CurrencyWeight { get; set; }

    /// <summary>
    /// What wallets will have money in them
    /// </summary>
    [JsonPropertyName("walletTplPool")]
    public List<string> WalletTplPool { get; set; }
}

public record EquipmentFilters
{
    /// <summary>
    /// Limits for mod types per weapon .e.g. scopes
    /// </summary>
    [JsonPropertyName("weaponModLimits")]
    public ModLimits WeaponModLimits { get; set; }

    /// <summary>
    /// Whitelist for weapon sight types allowed per gun
    /// </summary>
    [JsonPropertyName("weaponSightWhitelist")]
    public Dictionary<string, List<string>> WeaponSightWhitelist { get; set; }

    /// <summary>
    /// Chance face shield is down/active
    /// </summary>
    [JsonPropertyName("faceShieldIsActiveChancePercent")]
    public double? FaceShieldIsActiveChancePercent { get; set; }

    /// <summary>
    /// Chance gun flashlight is active during the day
    /// </summary>
    [JsonPropertyName("lightIsActiveDayChancePercent")]
    public double? LightIsActiveDayChancePercent { get; set; }

    /// <summary>
    /// Chance gun flashlight is active during the night
    /// </summary>
    [JsonPropertyName("lightIsActiveNightChancePercent")]
    public double? LightIsActiveNightChancePercent { get; set; }

    /// <summary>
    /// Chance gun laser is active during the day
    /// </summary>
    [JsonPropertyName("laserIsActiveChancePercent")]
    public double? LaserIsActiveChancePercent { get; set; }

    /// <summary>
    /// Chance NODS are down/active during the day
    /// </summary>
    [JsonPropertyName("nvgIsActiveChanceDayPercent")]
    public double? NvgIsActiveChanceDayPercent { get; set; }

    /// <summary>
    /// Chance NODS are down/active during the night
    /// </summary>
    [JsonPropertyName("nvgIsActiveChanceNightPercent")]
    public double? NvgIsActiveChanceNightPercent { get; set; }

    [JsonPropertyName("forceOnlyArmoredRigWhenNoArmor")]
    public bool? ForceOnlyArmoredRigWhenNoArmor { get; set; }

    /// <summary>
    /// Should plates be filtered by level
    /// </summary>
    [JsonPropertyName("filterPlatesByLevel")]
    public bool? FilterPlatesByLevel { get; set; }

    /// <summary>
    /// What additional slot ids should be seen as required when choosing a mod to add to a weapon
    /// </summary>
    [JsonPropertyName("weaponSlotIdsToMakeRequired")]
    public List<string>? WeaponSlotIdsToMakeRequired { get; set; }

    /// <summary>
    /// Adjust weighting/chances of items on bot by level of bot
    /// </summary>
    [JsonPropertyName("randomisation")]
    public List<RandomisationDetails> Randomisation { get; set; }

    /// <summary>
    /// Blacklist equipment by level of bot
    /// </summary>
    [JsonPropertyName("blacklist")]
    public List<EquipmentFilterDetails> Blacklist { get; set; }

    /// <summary>
    /// Whitelist equipment by level of bot
    /// </summary>
    [JsonPropertyName("whitelist")]
    public List<EquipmentFilterDetails> Whitelist { get; set; }

    /// <summary>
    /// Adjust equipment/ammo
    /// </summary>
    [JsonPropertyName("weightingAdjustmentsByBotLevel")]
    public List<WeightingAdjustmentDetails> WeightingAdjustmentsByBotLevel { get; set; }

    /// <summary>
    /// Same as weightingAdjustments but based on player level instead of bot level
    /// </summary>
    [JsonPropertyName("weightingAdjustmentsByPlayerLevel")]
    public List<WeightingAdjustmentDetails>? WeightingAdjustmentsByPlayerLevel { get; set; }

    /// <summary>
    /// Should the stock mod be forced to spawn on bot
    /// </summary>
    [JsonPropertyName("forceStock")]
    public bool? ForceStock { get; set; }

    [JsonPropertyName("armorPlateWeighting")]
    public List<ArmorPlateWeights>? ArmorPlateWeighting { get; set; }

    [JsonPropertyName("forceRigWhenNoVest")]
    public bool? ForceRigWhenNoVest { get; set; }
}

public record ModLimits
{
    /// <summary>
    /// How many scopes are allowed on a weapon - hard coded to work with OPTIC_SCOPE, ASSAULT_SCOPE, COLLIMATOR, COMPACT_COLLIMATOR
    /// </summary>
    [JsonPropertyName("scopeLimit")]
    public int? ScopeLimit { get; set; }

    /// <summary>
    /// How many lasers or lights are allowed on a weapon - hard coded to work with TACTICAL_COMBO, and FLASHLIGHT
    /// </summary>
    [JsonPropertyName("lightLaserLimit")]
    public int? LightLaserLimit { get; set; }
}

public record RandomisationDetails
{
    /// <summary>
    /// Between what levels do these randomisation setting apply to
    /// </summary>
    [JsonPropertyName("levelRange")]
    public MinMax LevelRange { get; set; }

    [JsonPropertyName("generation")]
    public Dictionary<string, GenerationData>? Generation { get; set; }

    /// <summary>
    /// Mod slots that should be fully randomised -ignores mods from bottype.json and instead creates a pool using items.json
    /// </summary>
    [JsonPropertyName("randomisedWeaponModSlots")]
    public List<string>? RandomisedWeaponModSlots { get; set; }

    /// <summary>
    /// Armor slots that should be randomised e.g. 'Headwear, Armband'
    /// </summary>
    [JsonPropertyName("randomisedArmorSlots")]
    public List<string>? RandomisedArmorSlots { get; set; }

    /// <summary>
    /// Equipment chances
    /// </summary>
    [JsonPropertyName("equipment")]
    public Dictionary<string, double>? Equipment { get; set; }

    /// <summary>
    /// Weapon mod chances
    /// </summary>
    [JsonPropertyName("weaponMods")]
    public Dictionary<string, double>? WeaponMods { get; set; }

    /// <summary>
    /// Equipment mod chances
    /// </summary>
    [JsonPropertyName("equipmentMods")]
    public Dictionary<string, double>? EquipmentMods { get; set; }

    [JsonPropertyName("nighttimeChanges")]
    public NighttimeChanges? NighttimeChanges { get; set; }

    /// <summary>
    /// Key = weapon tpl, value = min size of magazine allowed
    /// </summary>
    [JsonPropertyName("minimumMagazineSize")]
    public Dictionary<string, double>? MinimumMagazineSize { get; set; }
}

public record NighttimeChanges
{
    /// <summary>
    /// Applies changes to values stored in equipmentMods
    /// </summary>
    [JsonPropertyName("equipmentModsModifiers")]
    public Dictionary<string, float> EquipmentModsModifiers { get; set; }
    //public Dictionary<string, float> WeaponModsModifiers { get; set; } //TODO
}

public record EquipmentFilterDetails
{
    /// <summary>
    /// Between what levels do these equipment filter setting apply to
    /// </summary>
    [JsonPropertyName("levelRange")]
    public MinMax LevelRange { get; set; }

    /// <summary>
    /// Key: mod slot name e.g. mod_magazine, value: item tpls
    /// </summary>
    [JsonPropertyName("equipment")]
    public Dictionary<string, List<string>>? Equipment { get; set; }

    /// <summary>
    /// Key: equipment slot name e.g. FirstPrimaryWeapon, value: item tpls
    /// </summary>
    [JsonPropertyName("gear")]
    public Dictionary<EquipmentSlots, List<string>>? Gear { get; set; }

    /// <summary>
    /// Key: cartridge type e.g. Caliber23x75, value: item tpls
    /// </summary>
    [JsonPropertyName("cartridge")]
    public Dictionary<string, List<string>>? Cartridge { get; set; }
}

public record WeightingAdjustmentDetails
{
    /// <summary>
    /// Between what levels do these weight settings apply to
    /// </summary>
    [JsonPropertyName("levelRange")]
    public MinMax LevelRange { get; set; }

    /// <summary>
    /// Key: ammo type e.g. Caliber556x45NATO, value: item tpl + weight
    /// </summary>
    [JsonPropertyName("ammo")]
    public AdjustmentDetails? Ammo { get; set; }

    /// <summary>
    /// Key: equipment slot e.g. TacticalVest, value: item tpl + weight
    /// </summary>
    [JsonPropertyName("equipment")]
    public AdjustmentDetails? Equipment { get; set; }

    /// <summary>
    /// Key: clothing slot e.g. feet, value: item tpl + weight
    /// </summary>
    [JsonPropertyName("clothing")]
    public AdjustmentDetails? Clothing { get; set; }
}

public record AdjustmentDetails
{
    [JsonPropertyName("add")]
    public Dictionary<string, Dictionary<string, float>> Add { get; set; }

    [JsonPropertyName("edit")]
    public Dictionary<string, Dictionary<string, float>> Edit { get; set; }
}

public class ArmorPlateWeights
{
    [JsonPropertyName("levelRange")]
    public MinMax LevelRange { get; set; }

    public Dictionary<string, Dictionary<string, double>> Values { get; set; }
}

public record RandomisedResourceDetails
{
    [JsonPropertyName("food")]
    public RandomisedResourceValues Food { get; set; }

    [JsonPropertyName("meds")]
    public RandomisedResourceValues Meds { get; set; }
}

public record RandomisedResourceValues
{
    /// <summary>
    /// Minimum percent of item to randomized between min and max resource
    /// </summary>
    [JsonPropertyName("resourcePercent")]
    public float ResourcePercent { get; set; }

    /// <summary>
    /// Chance for randomization to not occur
    /// </summary>
    [JsonPropertyName("chanceMaxResourcePercent")]
    public float ChanceMaxResourcePercent { get; set; }
}
