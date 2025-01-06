using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Eft.Common;

namespace Core.Models.Spt.Config;

public class LocationConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-location";

    /// <summary>
    /// Rogues are classified as bosses and spawn immediately, this can result in no scavs spawning, delay rogues spawning to allow scavs to spawn first
    /// </summary>
    [JsonPropertyName("rogueLighthouseSpawnTimeSettings")]
    public RogueLighthouseSpawnTimeSettings RogueLighthouseSpawnTimeSettings { get; set; }

    /// <summary>
    /// When a map has hit max alive bots, any wave that should spawn will be reduced to 1 bot in size and placed in a spawn queue, this splits waves into smaller sizes to reduce the impact of this behaviour
    /// </summary>
    [JsonPropertyName("splitWaveIntoSingleSpawnsSettings")]
    public SplitWaveSettings SplitWaveIntoSingleSpawnsSettings { get; set; }

    [JsonPropertyName("looseLootMultiplier")]
    public LootMultiplier LooseLootMultiplier { get; set; }
    
    [JsonPropertyName("staticLootMultiplier")]
    public LootMultiplier StaticLootMultiplier { get; set; }

    /// <summary>
    /// Custom bot waves to add to a locations base json on game start if addCustomBotWavesToMaps is true
    /// </summary>
    [JsonPropertyName("customWaves")]
    public CustomWaves CustomWaves { get; set; }

    /// <summary>
    /// Open zones to add to map
    /// </summary>
    [JsonPropertyName("openZones")]
    public Dictionary<string, List<string>> OpenZones { get; set; }

    /// <summary>
    /// Key = map id, value = item tpls that should only have one forced loot spawn position
    /// </summary>
    [JsonPropertyName("forcedLootSingleSpawnById")]
    public Dictionary<string, List<string>> ForcedLootSingleSpawnById { get; set; }

    /// <summary>
    /// How many attempts should be taken to fit an item into a container before giving up
    /// </summary>
    [JsonPropertyName("fitLootIntoContainerAttempts")]
    public int FitLootIntoContainerAttempts { get; set; }

    /// <summary>
    /// Add all possible zones to each maps `OpenZones` property
    /// </summary>
    [JsonPropertyName("addOpenZonesToAllMaps")]
    public bool AddOpenZonesToAllMaps { get; set; }

    /// <summary>
    /// Allow addition of custom bot waves designed by SPT to be added to maps - defined in configs/location.json.customWaves
    /// </summary>
    [JsonPropertyName("addCustomBotWavesToMaps")]
    public bool AddCustomBotWavesToMaps { get; set; }

    /// <summary>
    /// Should the limits defined inside botTypeLimits to be applied to locations on game start
    /// </summary>
    [JsonPropertyName("enableBotTypeLimits")]
    public bool EnableBotTypeLimits { get; set; }

    /// <summary>
    /// Add limits to a locations base.MinMaxBots array if enableBotTypeLimits is true
    /// </summary>
    [JsonPropertyName("botTypeLimits")]
    public Dictionary<string, List<BotTypeLimit>> BotTypeLimits { get; set; }
    
    /// <summary>
    /// Container randomisation settings
    /// </summary>
    [JsonPropertyName("containerRandomisationSettings")]
    public ContainerRandomisationSettings ContainerRandomisationSettings { get; set; }

    /// <summary>
    /// How full must a random loose magazine be %
    /// </summary>
    [JsonPropertyName("minFillLooseMagazinePercent")]
    public int MinFillLooseMagazinePercent { get; set; }

    /// <summary>
    /// How full must a random static magazine be %
    /// </summary>
    [JsonPropertyName("minFillStaticMagazinePercent")]
    public int MinFillStaticMagazinePercent { get; set; }

    [JsonPropertyName("allowDuplicateItemsInStaticContainers")]
    public bool AllowDuplicateItemsInStaticContainers { get; set; }

    /// <summary>
    /// Chance loose magazines have ammo in them TODO - rename to dynamicMagazineLootHasAmmoChancePercent
    /// </summary>
    [JsonPropertyName("magazineLootHasAmmoChancePercent")]
    public int MagazineLootHasAmmoChancePercent { get; set; }

    /// <summary>
    /// Chance static magazines have ammo in them
    /// </summary>
    [JsonPropertyName("staticMagazineLootHasAmmoChancePercent")]
    public int StaticMagazineLootHasAmmoChancePercent { get; set; }

    /// <summary>
    /// Key: map, value: loose loot ids to ignore
    /// </summary>
    [JsonPropertyName("looseLootBlacklist")]
    public Dictionary<string, List<string>> LooseLootBlacklist { get; set; }

    /// <summary>
    /// Key: map, value: settings to control how long scav raids are
    /// </summary>
    [JsonPropertyName("scavRaidTimeSettings")]
    public ScavRaidTimeSettings ScavRaidTimeSettings { get; set; }

    /// <summary>
    /// Settings to adjust mods for lootable equipment in raid
    /// </summary>
    [JsonPropertyName("equipmentLootSettings")]
    public EquipmentLootSettings EquipmentLootSettings { get; set; }

    /// <summary>
    /// Min percentage to set raider spawns at, -1 makes no changes
    /// </summary>
    [JsonPropertyName("reserveRaiderSpawnChanceOverrides")]
    public ReserveRaiderSpawnChanceOverrides ReserveRaiderSpawnChanceOverrides { get; set; }

    /// <summary>
    /// Map ids players cannot visit
    /// </summary>
    [JsonPropertyName("nonMaps")]
    public List<string> NonMaps { get; set; }
}

public class ReserveRaiderSpawnChanceOverrides
{
    [JsonPropertyName("nonTriggered")]
    public int NonTriggered { get; set; }
    
    [JsonPropertyName("triggered")]
    public int Triggered { get; set; }
}

public class EquipmentLootSettings
{
    // Percentage chance item will be added to equipment
    [JsonPropertyName("modSpawnChancePercent")]
    public Dictionary<string, int> ModSpawnChancePercent { get; set; }
}

public class FixEmptyBotWavesSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
    
    [JsonPropertyName("ignoreMaps")]
    public List<string> IgnoreMaps { get; set; }
}

public class RogueLighthouseSpawnTimeSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
    
    [JsonPropertyName("waitTimeSeconds")]
    public int WaitTimeSeconds { get; set; }
}

public class SplitWaveSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
    
    [JsonPropertyName("ignoreMaps")]
    public List<string> IgnoreMaps { get; set; }
    
    [JsonPropertyName("waveSizeThreshold")]
    public int WaveSizeThreshold { get; set; }
}

public class CustomWaves
{
    /** Bosses spawn on raid start */
    [JsonPropertyName("boss")]
    public Dictionary<string, List<BossLocationSpawn>> Boss { get; set; }
    
    [JsonPropertyName("normal")]
    public Dictionary<string, List<Wave>> Normal { get; set; }
}

public class BotTypeLimit : MinMax
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}

/** Multiplier to apply to the loot count for a given map */
public class LootMultiplier
{
    [JsonPropertyName("bigmap")]
    public int BigMap { get; set; }
    
    [JsonPropertyName("develop")]
    public int Develop { get; set; }
    
    [JsonPropertyName("factory4_day")]
    public int Factory4Day { get; set; }
    
    [JsonPropertyName("factory4_night")]
    public int Factory4Night { get; set; }
    
    [JsonPropertyName("interchange")]
    public int Interchange { get; set; }
    
    [JsonPropertyName("laboratory")]
    public int Laboratory { get; set; }
    
    [JsonPropertyName("rezervbase")]
    public int RezervBase { get; set; }
    
    [JsonPropertyName("shoreline")]
    public int Shoreline { get; set; }
    
    [JsonPropertyName("woods")]
    public int Woods { get; set; }
    
    [JsonPropertyName("hideout")]
    public int Hideout { get; set; }
    
    [JsonPropertyName("lighthouse")]
    public int Lighthouse { get; set; }
    
    [JsonPropertyName("privatearea")]
    public int PrivateArea { get; set; }
    
    [JsonPropertyName("suburbs")]
    public int Suburbs { get; set; }
    
    [JsonPropertyName("tarkovstreets")]
    public int TarkovStreets { get; set; }
    
    [JsonPropertyName("terminal")]
    public int Terminal { get; set; }
    
    [JsonPropertyName("town")]
    public int Town { get; set; }
    
    [JsonPropertyName("sandbox")]
    public int Sandbox { get; set; }
}

public class ContainerRandomisationSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
    
    /** What maps can use the container randomisation feature */
    [JsonPropertyName("maps")]
    public Dictionary<string, bool> Maps { get; set; }
    
    /** Some container types don't work when randomised */
    [JsonPropertyName("containerTypesToNotRandomise")]
    public List<string> ContainerTypesToNotRandomise { get; set; }
    
    [JsonPropertyName("containerGroupMinSizeMultiplier")]
    public double ContainerGroupMinSizeMultiplier { get; set; }
    
    [JsonPropertyName("containerGroupMaxSizeMultiplier")]
    public double ContainerGroupMaxSizeMultiplier { get; set; }
}

public class ScavRaidTimeSettings
{
    [JsonPropertyName("settings")]
    public ScavRaidTimeConfigSettings Settings { get; set; }
    
    [JsonPropertyName("maps")]
    public Dictionary<string, ScavRaidTimeLocationSettings> Maps { get; set; }
}

public class ScavRaidTimeConfigSettings
{
    [JsonPropertyName("trainArrivalDelayObservedSeconds")]
    public int TrainArrivalDelayObservedSeconds { get; set; }
}

public class ScavRaidTimeLocationSettings
{
    /** Should loot be reduced by same percent length of raid is reduced by */
    [JsonPropertyName("reduceLootByPercent")]
    public bool ReduceLootByPercent { get; set; }
    
    /** Smallest % of container loot that should be spawned */
    [JsonPropertyName("minStaticLootPercent")]
    public double MinStaticLootPercent { get; set; }
    
    /** Smallest % of loose loot that should be spawned */
    [JsonPropertyName("minDynamicLootPercent")]
    public double MinDynamicLootPercent { get; set; }
    
    /** Chance raid time is reduced */
    [JsonPropertyName("reducedChancePercent")]
    public double ReducedChancePercent { get; set; }
    
    /** How much should raid time be reduced - weighted */
    [JsonPropertyName("reductionPercentWeights")]
    public Dictionary<string, double> ReductionPercentWeights { get; set; }
    
    /** Should bot waves be removed / spawn times be adjusted */
    [JsonPropertyName("adjustWaves")]
    public bool AdjustWaves { get; set; }
}