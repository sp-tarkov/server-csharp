using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record LocationConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind { get; set; } = "spt-location";

    /// <summary>
    ///     Rogues are classified as bosses and spawn immediately, this can result in no scavs spawning, delay rogues spawning to allow scavs to spawn first
    /// </summary>
    [JsonPropertyName("rogueLighthouseSpawnTimeSettings")]
    public required RogueLighthouseSpawnTimeSettings RogueLighthouseSpawnTimeSettings { get; set; }

    [JsonPropertyName("looseLootMultiplier")]
    public required Dictionary<string, double> LooseLootMultiplier { get; set; }

    [JsonPropertyName("staticLootMultiplier")]
    public required Dictionary<string, double> StaticLootMultiplier { get; set; }

    /// <summary>
    ///     Custom bot waves to add to a locations base json on game start if addCustomBotWavesToMaps is true
    /// </summary>
    [JsonPropertyName("customWaves")]
    public CustomWaves? CustomWaves { get; set; }

    /// <summary>
    ///     Open zones to add to map
    /// </summary>
    [JsonPropertyName("openZones")]
    public required Dictionary<string, HashSet<string>> OpenZones { get; set; }

    /// <summary>
    ///     Key = map id, value = dict of item tpls that should only have x forced loot spawn position
    /// </summary>
    [JsonPropertyName("lootMaxSpawnLimits")]
    public required Dictionary<string, Dictionary<string, int>> LootMaxSpawnLimits { get; set; }

    /// <summary>
    ///     How many attempts should be taken to fit an item into a container before giving up
    /// </summary>
    [JsonPropertyName("fitLootIntoContainerAttempts")]
    public int FitLootIntoContainerAttempts { get; set; }

    /// <summary>
    ///     Add all possible zones to each maps `OpenZones` property
    /// </summary>
    [JsonPropertyName("addOpenZonesToAllMaps")]
    public bool AddOpenZonesToAllMaps { get; set; }

    /// <summary>
    ///     Allow addition of custom bot waves designed by SPT to be added to maps - defined in configs/location.json.customWaves
    /// </summary>
    [JsonPropertyName("addCustomBotWavesToMaps")]
    public bool AddCustomBotWavesToMaps { get; set; }

    /// <summary>
    ///     Should the limits defined inside botTypeLimits to be applied to locations on game start
    /// </summary>
    [JsonPropertyName("enableBotTypeLimits")]
    public bool EnableBotTypeLimits { get; set; }

    /// <summary>
    ///     Add limits to a locations base.MinMaxBots array if enableBotTypeLimits is true
    /// </summary>
    [JsonPropertyName("botTypeLimits")]
    public required Dictionary<string, List<BotTypeLimit>> BotTypeLimits { get; set; }

    /// <summary>
    ///     Container randomisation settings
    /// </summary>
    [JsonPropertyName("containerRandomisationSettings")]
    public required ContainerRandomisationSettings ContainerRandomisationSettings { get; set; }

    /// <summary>
    ///     How full must a random loose magazine be %
    /// </summary>
    [JsonPropertyName("minFillLooseMagazinePercent")]
    public int MinFillLooseMagazinePercent { get; set; }

    /// <summary>
    ///     How full must a random static magazine be %
    /// </summary>
    [JsonPropertyName("minFillStaticMagazinePercent")]
    public int MinFillStaticMagazinePercent { get; set; }

    [JsonPropertyName("allowDuplicateItemsInStaticContainers")]
    public bool AllowDuplicateItemsInStaticContainers { get; set; }

    /// <summary>
    ///     Chance loose magazines have ammo in them TODO - rename to dynamicMagazineLootHasAmmoChancePercent
    /// </summary>
    [JsonPropertyName("magazineLootHasAmmoChancePercent")]
    public int MagazineLootHasAmmoChancePercent { get; set; }

    /// <summary>
    ///     Chance static magazines have ammo in them
    /// </summary>
    [JsonPropertyName("staticMagazineLootHasAmmoChancePercent")]
    public int StaticMagazineLootHasAmmoChancePercent { get; set; }

    /// <summary>
    ///     Key: map, value: loose loot ids to ignore
    /// </summary>
    [JsonPropertyName("looseLootBlacklist")]
    public required Dictionary<string, List<string>> LooseLootBlacklist { get; set; }

    /// <summary>
    ///     Key: map, value: settings to control how long scav raids are
    /// </summary>
    [JsonPropertyName("scavRaidTimeSettings")]
    public required ScavRaidTimeSettings ScavRaidTimeSettings { get; set; }

    /// <summary>
    ///     Settings to adjust mods for lootable equipment in raid
    /// </summary>
    [JsonPropertyName("equipmentLootSettings")]
    public required EquipmentLootSettings EquipmentLootSettings { get; set; }

    /// <summary>
    ///     Min percentage to set raider spawns at, -1 makes no changes
    /// </summary>
    [JsonPropertyName("reserveRaiderSpawnChanceOverrides")]
    public required ReserveRaiderSpawnChanceOverrides ReserveRaiderSpawnChanceOverrides { get; set; }

    /// <summary>
    ///     Containers to remove all children from when generating static/loose loot
    /// </summary>
    [JsonPropertyName("tplsToStripChildItemsFrom")]
    public required HashSet<string> TplsToStripChildItemsFrom { get; set; }

    /// <summary>
    ///     Map ids players cannot visit
    /// </summary>
    [JsonPropertyName("nonMaps")]
    public required HashSet<string> NonMaps { get; set; }

    [JsonPropertyName("transitSettings")]
    public TransitSettings? TransitSettings { get; set; }
}

public record TransitSettings
{
    [JsonPropertyName("effectsToRemove")]
    public HashSet<string>? EffectsToRemove { get; set; }

    [JsonPropertyName("adjustLimbHealthPoints")]
    public bool? AdjustLimbHealthPoints { get; set; }

    [JsonPropertyName("limbHealPercent")]
    public int? LimbHealPercent { get; set; }
}

public record ReserveRaiderSpawnChanceOverrides
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("nonTriggered")]
    public int NonTriggered { get; set; }

    [JsonPropertyName("triggered")]
    public int Triggered { get; set; }
}

public record EquipmentLootSettings
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    /// <summary>
    ///     Percentage chance item will be added to equipment
    /// </summary>
    [JsonPropertyName("modSpawnChancePercent")]
    public required Dictionary<string, double> ModSpawnChancePercent { get; set; }
}

public record RogueLighthouseSpawnTimeSettings
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("waitTimeSeconds")]
    public int WaitTimeSeconds { get; set; }
}

public record CustomWaves
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    /// <summary>
    ///     Bosses spawn on raid start
    /// </summary>
    [JsonPropertyName("boss")]
    public Dictionary<string, List<BossLocationSpawn>> Boss { get; set; } = [];

    [JsonPropertyName("normal")]
    public Dictionary<string, List<Wave>> Normal { get; set; } = [];
}

public record BotTypeLimit : MinMax<int>
{
    [JsonPropertyName("type")]
    public new required string Type { get; set; }
}

public record ContainerRandomisationSettings
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    /// <summary>
    ///     What maps can use the container randomisation feature
    /// </summary>
    [JsonPropertyName("maps")]
    public required Dictionary<string, bool> Maps { get; set; }

    /// <summary>
    ///     Some container types don't work when randomised
    /// </summary>
    [JsonPropertyName("containerTypesToNotRandomise")]
    public required HashSet<string> ContainerTypesToNotRandomise { get; set; }

    [JsonPropertyName("containerGroupMinSizeMultiplier")]
    public double ContainerGroupMinSizeMultiplier { get; set; }

    [JsonPropertyName("containerGroupMaxSizeMultiplier")]
    public double ContainerGroupMaxSizeMultiplier { get; set; }
}

public record ScavRaidTimeSettings
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("settings")]
    public required ScavRaidTimeConfigSettings Settings { get; set; }

    [JsonPropertyName("maps")]
    public required Dictionary<string, ScavRaidTimeLocationSettings?> Maps { get; set; }
}

public record ScavRaidTimeConfigSettings
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("trainArrivalDelayObservedSeconds")]
    public int TrainArrivalDelayObservedSeconds { get; set; }
}

public record ScavRaidTimeLocationSettings
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    /// <summary>
    ///     Should loot be reduced by same percent length of raid is reduced by
    /// </summary>
    [JsonPropertyName("reduceLootByPercent")]
    public bool ReduceLootByPercent { get; set; }

    /// <summary>
    ///     Smallest % of container loot that should be spawned
    /// </summary>
    [JsonPropertyName("minStaticLootPercent")]
    public double MinStaticLootPercent { get; set; }

    /// <summary>
    ///     Smallest % of loose loot that should be spawned
    /// </summary>
    [JsonPropertyName("minDynamicLootPercent")]
    public double MinDynamicLootPercent { get; set; }

    /// <summary>
    ///     Chance raid time is reduced
    /// </summary>
    [JsonPropertyName("reducedChancePercent")]
    public double ReducedChancePercent { get; set; }

    /// <summary>
    ///     How much should raid time be reduced - weighted
    /// </summary>
    [JsonPropertyName("reductionPercentWeights")]
    public Dictionary<string, double> ReductionPercentWeights { get; set; } = [];

    /// <summary>
    ///     Should bot waves be removed / spawn times be adjusted
    /// </summary>
    [JsonPropertyName("adjustWaves")]
    public bool AdjustWaves { get; set; }
}
