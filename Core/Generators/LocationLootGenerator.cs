using System.Text.Json.Serialization;
using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Config;

namespace Core.Generators;

[Injectable]
public class LocationLootGenerator
{
    private LocationConfig _locationConfig;
    private SeasonalEventConfig _seasonalEventConfig;

    public LocationLootGenerator()
    {
    }

    /// Create a list of container objects with randomised loot
    /// </summary>
    /// <param name="locationBase">Map base to generate containers for</param>
    /// <param name="staticAmmoDist">Static ammo distribution</param>
    /// <returns>List of container objects</returns>
    public List<SpawnpointTemplate> GenerateStaticContainers(LocationBase locationBase, Dictionary<string, List<StaticAmmoDetails>> staticAmmoDist)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get containers with a non-100% chance to spawn OR are NOT on the container type randomistion blacklist
    /// </summary>
    /// <param name="staticContainers"></param>
    /// <returns>StaticContainerData array</returns>
    protected List<StaticContainerData> GetRandomisableContainersOnMap(List<StaticContainerData> staticContainers)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get containers with 100% spawn rate or have a type on the randomistion ignore list
    /// </summary>
    /// <param name="staticContainersOnMap"></param>
    /// <returns>IStaticContainerData array</returns>
    protected List<StaticContainerData> GetGuaranteedContainers(List<StaticContainerData> staticContainersOnMap)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose a number of containers based on their probabilty value to fulfil the desired count in containerData.chosenCount
    /// </summary>
    /// <param name="groupId">Name of the group the containers are being collected for</param>
    /// <param name="containerData">Containers and probability values for a groupId</param>
    /// <returns>List of chosen container Ids</returns>
    protected List<string> GetContainersByProbabilty(string groupId, ContainerGroupCount containerData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a mapping of each groupid and the containers in that group + count of containers to spawn on map
    /// </summary>
    /// <param name="containersGroups">Container group values</param>
    /// <returns>dictionary keyed by groupId</returns>
    protected Dictionary<string, ContainerGroupCount> GetGroupIdToContainerMappings(
        object staticContainerGroupData, // TODO: Type fuckery staticContainerGroupData was IStaticContainer | Record<string, IContainerMinMax>
        List<StaticContainerData> staticContainersOnMap)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose loot to put into a static container based on weighting
    /// Handle forced items + seasonal item removal when not in season
    /// </summary>
    /// <param name="staticContainer">The container itself we will add loot to</param>
    /// <param name="staticForced">Loot we need to force into the container</param>
    /// <param name="staticLootDist">staticLoot.json</param>
    /// <param name="staticAmmoDist">staticAmmo.json</param>
    /// <param name="locationName">Name of the map to generate static loot for</param>
    /// <returns>StaticContainerData</returns>
    protected StaticContainerData AddLootToContainer(StaticContainerData staticContainer, List<StaticForcedProps> staticForced,
        Dictionary<string, StaticLootDetails> staticLootDist, Dictionary<string, List<StaticAmmoDetails>> staticAmmoDist, string locationName
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a 2D grid of a container's item slots
    /// </summary>
    /// <param name="containerTpl">Tpl id of the container</param>
    /// <returns>List<List<int>></returns>
    protected List<List<int>> GetContainerMapping(string containerTpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Look up a containers itemcountDistribution data and choose an item count based on the found weights
    /// </summary>
    /// <param name="containerTypeId">Container to get item count for</param>
    /// <param name="staticLootDist">staticLoot.json</param>
    /// <param name="locationName">Map name (to get per-map multiplier for from config)</param>
    /// <returns>item count</returns>
    protected int GetWeightedCountOfContainerItems(string containerTypeId, Dictionary<string, StaticLootDetails> staticLootDist, string locationName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get all possible loot items that can be placed into a container
    /// Do not add seasonal items if found + current date is inside seasonal event
    /// </summary>
    /// <param name="containerTypeId">Container to get possible loot for</param>
    /// <param name="staticLootDist">staticLoot.json</param>
    /// <returns>ProbabilityObjectArray of item tpls + probabilty</returns>
    protected object GetPossibleLootItemsForContainer(string containerTypeId,
        Dictionary<string, StaticLootDetails> staticLootDist) // TODO: Type Fuckery, return type was ProbabilityObjectArray<string, number>
    {
        throw new NotImplementedException();
    }

    protected double GetLooseLootMultiplerForLocation(string location)
    {
        throw new NotImplementedException();
    }

    protected double GetStaticLootMultiplierForLocation(string location)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create array of loose + forced loot using probability system
    /// </summary>
    /// <param name="dynamicLootDist"></param>
    /// <param name="staticAmmoDist"></param>
    /// <param name="locationName">Location to generate loot for</param>
    /// <returns>Array of spawn points with loot in them</returns>
    public List<SpawnpointTemplate> GenerateDynamicLoot(LooseLoot dynamicLootDist, Dictionary<string, List<StaticAmmoDetails>> staticAmmoDist,
        string locationName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add forced spawn point loot into loot parameter list
    /// </summary>
    /// <param name="lootLocationTemplates">List to add forced loot spawn locations to</param>
    /// <param name="forcedSpawnPoints">Forced loot locations that must be added</param>
    /// <param name="locationName">Name of map currently having force loot created for</param>
    protected void addForcedLoot(List<SpawnpointTemplate> lootLocationTemplates, List<SpawnpointsForced> forcedSpawnPoints, string locationName,
        Dictionary<string, List<StaticAmmoDetails>> staticAmmoDist)
    {
        throw new NotImplementedException();
    }

    // TODO: rewrite, BIG yikes
    protected ContainerItem CreateStaticLootItem(string chosenTemplate, Dictionary<string, List<StaticAmmoDetails>> staticAmmoDistribution,
        string? parentIdentifier = null)
    {
        throw new NotImplementedException();
    }
}

public class ContainerGroupCount
{
    [JsonPropertyName("containerIdsWithProbability")]
    public Dictionary<string, double> ContainerIdsWithProbability { get; set; }

    [JsonPropertyName("chosenCount")]
    public int ChosenCount { get; set; }
}

public class ContainerItem
{
    [JsonPropertyName("items")]
    public List<Item> Items { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}
