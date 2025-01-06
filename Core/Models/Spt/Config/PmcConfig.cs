using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Eft.Common;
using Core.Models.Enums;

namespace Core.Models.Spt.Config;

public class PmcConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-pmc";
    
    /** What game version should the PMC have */
    [JsonPropertyName("gameVersionWeight")]
    public Dictionary<string, double> GameVersionWeight { get; set; }
    
    /** What account type should the PMC have */
    [JsonPropertyName("accountTypeWeight")]
    public Dictionary<MemberCategory, double> AccountTypeWeight { get; set; }
    
    /** Global whitelist/blacklist of vest loot for PMCs */
    [JsonPropertyName("vestLoot")]
    public SlotLootSettings VestLoot { get; set; }
    
    /** Global whitelist/blacklist of pocket loot for PMCs */
    [JsonPropertyName("pocketLoot")]
    public SlotLootSettings PocketLoot { get; set; }
    
    /** Global whitelist/blacklist of backpack loot for PMCs */
    [JsonPropertyName("backpackLoot")]
    public SlotLootSettings BackpackLoot { get; set; }
    
    [JsonPropertyName("globalLootBlacklist")]
    public List<string> GlobalLootBlacklist { get; set; }
    
    /** Use difficulty defined in config/bot.json/difficulty instead of chosen difficulty dropdown value */
    [JsonPropertyName("useDifficultyOverride")]
    public bool UseDifficultyOverride { get; set; }
    
    /** Difficulty override e.g. "AsOnline/Hard" */
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; }
    
    /** Chance out of 100 to have a complete gun in backpack */
    [JsonPropertyName("looseWeaponInBackpackChancePercent")]
    public double LooseWeaponInBackpackChancePercent { get; set; }
    
    /** Chance out of 100 to have an enhancement applied to PMC weapon */
    [JsonPropertyName("weaponHasEnhancementChancePercent")]
    public double WeaponHasEnhancementChancePercent { get; set; }
    
    /** MinMax count of weapons to have in backpack */
    [JsonPropertyName("looseWeaponInBackpackLootMinMax")]
    public MinMax LooseWeaponInBackpackLootMinMax { get; set; }
    
    /** Percentage chance PMC will be USEC */
    [JsonPropertyName("isUsec")]
    public double IsUsec { get; set; }
    
    /** WildSpawnType enum value USEC PMCs use */
    [JsonPropertyName("usecType")]
    public string UsecType { get; set; }
    
    /** WildSpawnType enum value BEAR PMCs use */
    [JsonPropertyName("bearType")]
    public string BearType { get; set; }
    
    /** What 'brain' does a PMC use, keyed by map and side (USEC/BEAR) key: map location, value: type for usec/bear */
    [JsonPropertyName("pmcType")]
    public Dictionary<string, Dictionary<string, Dictionary<string, double>>> PmcType { get; set; }
    
    [JsonPropertyName("maxBackpackLootTotalRub")]
    public List<IMinMaxLootValue> MaxBackpackLootTotalRub { get; set; }
    
    [JsonPropertyName("maxPocketLootTotalRub")]
    public double MaxPocketLootTotalRub { get; set; }
    
    [JsonPropertyName("maxVestLootTotalRub")]
    public double MaxVestLootTotalRub { get; set; }
    
    /** Percentage chance a bot from a wave is converted into a PMC, first key = map, second key = bot wildspawn type (assault/exusec), value: min+max chance to be converted */
    [JsonPropertyName("convertIntoPmcChance")]
    public Dictionary<string, Dictionary<string, MinMax>> ConvertIntoPmcChance { get; set; }
    
    /** How many levels above player level can a PMC be */
    [JsonPropertyName("botRelativeLevelDeltaMax")]
    public double BotRelativeLevelDeltaMax { get; set; }
    
    /** How many levels below player level can a PMC be */
    [JsonPropertyName("botRelativeLevelDeltaMin")]
    public double BotRelativeLevelDeltaMin { get; set; }
    
    /** Force a number of healing items into PMCs secure container to ensure they can heal */
    [JsonPropertyName("forceHealingItemsIntoSecure")]
    public bool ForceHealingItemsIntoSecure { get; set; }
    
    [JsonPropertyName("hostilitySettings")]
    public Dictionary<string, HostilitySettings> HostilitySettings { get; set; }
    
    [JsonPropertyName("allPMCsHavePlayerNameWithRandomPrefixChance")]
    public double AllPMCsHavePlayerNameWithRandomPrefixChance { get; set; }
    
    [JsonPropertyName("locationSpecificPmcLevelOverride")]
    public Dictionary<string, MinMax> LocationSpecificPmcLevelOverride { get; set; }
    
    /** Should secure container loot from usec.json/bear.json be added to pmc bots secure */
    [JsonPropertyName("addSecureContainerLootFromBotConfig")]
    public bool AddSecureContainerLootFromBotConfig { get; set; }
}

public class HostilitySettings
{
    /** Bot roles that are 100% an enemy */
    [JsonPropertyName("additionalEnemyTypes")]
    public List<string>? AdditionalEnemyTypes { get; set; }
    
    /** Objects that determine the % chance another bot type is an enemy */
    [JsonPropertyName("chancedEnemies")]
    public List<ChancedEnemy>? ChancedEnemies { get; set; }
    
    [JsonPropertyName("bearEnemyChance")]
    public double? BearEnemyChance { get; set; }
    
    [JsonPropertyName("usecEnemyChance")]
    public double? UsecEnemyChance { get; set; }
    
    [JsonPropertyName("savageEnemyChance")]
    public double? SavageEnemyChance { get; set; }
    
    /** Bot roles that are 100% a friendly */
    [JsonPropertyName("additionalFriendlyTypes")]
    public List<string>? AdditionalFriendlyTypes { get; set; }
    
    [JsonPropertyName("savagePlayerBehaviour")]
    public string? SavagePlayerBehaviour { get; set; }
}

public class PmcTypes
{
    [JsonPropertyName("usec")]
    public string Usec { get; set; }
    
    [JsonPropertyName("bear")]
    public string Bear { get; set; }
}

public class SlotLootSettings
{
    /** Item Type whitelist */
    [JsonPropertyName("whitelist")]
    public List<string> Whitelist { get; set; }
    
    /** Item tpl blacklist */
    [JsonPropertyName("blacklist")]
    public List<string> Blacklist { get; set; }
}

public class IMinMaxLootValue : MinMax
{
    [JsonPropertyName("value")]
    public double Value { get; set; }
}