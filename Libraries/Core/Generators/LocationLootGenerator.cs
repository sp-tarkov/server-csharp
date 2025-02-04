using System.Text.Json.Serialization;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using Core.Utils.Collections;
using SptCommon.Annotations;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Generators;

[Injectable]
public class LocationLootGenerator(
    ISptLogger<LocationLootGenerator> _logger,
    RandomUtil _randomUtil,
    MathUtil _mathUtil,
    HashUtil _hashUtil,
    ItemHelper _itemHelper,
    InventoryHelper _inventoryHelper,
    DatabaseService _databaseService,
    ContainerHelper _containerHelper,
    PresetHelper _presetHelper,
    LocalisationService _localisationService,
    SeasonalEventService _seasonalEventService,
    ItemFilterService _itemFilterService,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected LocationConfig _locationConfig = _configServer.GetConfig<LocationConfig>();
    protected SeasonalEventConfig _seasonalEventConfig = _configServer.GetConfig<SeasonalEventConfig>();

    /// Create a list of container objects with randomised loot
    /// <param name="locationBase">Map base to generate containers for</param>
    /// <param name="staticAmmoDist">Static ammo distribution</param>
    /// <returns>List of container objects</returns>
    public List<SpawnpointTemplate> GenerateStaticContainers(LocationBase locationBase,
        Dictionary<string, List<StaticAmmoDetails>> staticAmmoDist)
    {
        var staticLootItemCount = 0;
        var result = new List<SpawnpointTemplate>();
        var locationId = locationBase.Id.ToLower();

        var mapData = _databaseService.GetLocation(locationId);

        var staticWeaponsOnMapClone = _cloner.Clone(mapData.StaticContainers.Value.StaticWeapons);
        if (staticWeaponsOnMapClone is null)
        {
            _logger.Error(
                _localisationService.GetText("location-unable_to_find_static_weapon_for_map", locationBase.Name)
            );
        }

        // Add mounted weapons to output loot
        result.AddRange(staticWeaponsOnMapClone);

        var allStaticContainersOnMapClone = _cloner.Clone(mapData.StaticContainers.Value.StaticContainers);
        if (allStaticContainersOnMapClone is null)
        {
            _logger.Error(
                _localisationService.GetText("location-unable_to_find_static_container_for_map", locationBase.Name)
            );
        }

        // Containers that MUST be added to map (e.g. quest containers)
        var staticForcedOnMapClone = _cloner.Clone(mapData.StaticContainers.Value.StaticForced);
        if (staticForcedOnMapClone is null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "location-unable_to_find_forced_static_data_for_map",
                    locationBase.Name
                )
            );
        }

        // Remove christmas items from loot data
        if (!_seasonalEventService.ChristmasEventEnabled())
        {
            allStaticContainersOnMapClone = allStaticContainersOnMapClone.Where(
                    item => !_seasonalEventConfig.ChristmasContainerIds.Contains(item.Template.Id)
                )
                .ToList();
        }

        var staticRandomisableContainersOnMap = GetRandomisableContainersOnMap(allStaticContainersOnMapClone);

        // Keep track of static loot count
        var staticContainerCount = 0;

        // Find all 100% spawn containers
        var staticLootDist = mapData.StaticLoot;
        var guaranteedContainers = GetGuaranteedContainers(allStaticContainersOnMapClone);
        staticContainerCount += guaranteedContainers.Count;

        // Add loot to guaranteed containers and add to result
        foreach (var container in guaranteedContainers)
        {
            var containerWithLoot = AddLootToContainer(
                container,
                staticForcedOnMapClone,
                staticLootDist.Value,
                staticAmmoDist,
                locationId
            );
            result.Add(containerWithLoot.Template);

            staticLootItemCount += containerWithLoot.Template.Items.Count;
        }

        if(_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Added {guaranteedContainers.Count} guaranteed containers");
        }

        // Randomisation is turned off globally or just turned off for this map
        if (
            !(
                _locationConfig.ContainerRandomisationSettings.Enabled &&
                _locationConfig.ContainerRandomisationSettings.Maps[locationId]
            )
        )
        {
            if(_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug(
                    $"Container randomisation disabled, Adding {staticRandomisableContainersOnMap.Count} containers to {locationBase.Name}"
                );
            }
            foreach (var container in staticRandomisableContainersOnMap)
            {
                var containerWithLoot = AddLootToContainer(
                    container,
                    staticForcedOnMapClone,
                    staticLootDist.Value,
                    staticAmmoDist,
                    locationId
                );
                result.Add(containerWithLoot.Template);

                staticLootItemCount += containerWithLoot.Template.Items.Count;
            }

            _logger.Success($"A total of {staticLootItemCount} static items spawned");

            return result;
        }

        // Group containers by their groupId
        if (mapData.Statics is null)
        {
            _logger.Warning(_localisationService.GetText("location-unable_to_generate_static_loot", locationId));

            return result;
        }

        var mapping = GetGroupIdToContainerMappings(mapData.Statics, staticRandomisableContainersOnMap);

        // For each of the container groups, choose from the pool of containers, hydrate container with loot and add to result array
        foreach (var (key, data) in mapping)
        {
            // Count chosen was 0, skip
            if (data.ChosenCount == 0)
            {
                continue;
            }

            if (data.ContainerIdsWithProbability.Count == 0)
            {
                if(_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"Group: {key} has no containers with < 100 % spawn chance to choose from, skipping");
                }

                continue;
            }

            // EDGE CASE: These are containers without a group and have a probability < 100%
            if (key == "")
            {
                var containerIdsCopy = _cloner.Clone(data.ContainerIdsWithProbability);
                // Roll each containers probability, if it passes, it gets added
                data.ContainerIdsWithProbability = new Dictionary<string, double>();
                foreach (var containerId in containerIdsCopy)
                    if (_randomUtil.GetChance100(containerIdsCopy[containerId.Key] * 100))
                    {
                        data.ContainerIdsWithProbability[containerId.Key] = containerIdsCopy[containerId.Key];
                    }

                // Set desired count to size of array (we want all containers chosen)
                data.ChosenCount = data.ContainerIdsWithProbability.Count;

                // EDGE CASE: chosen container count could be 0
                if (data.ChosenCount == 0)
                {
                    continue;
                }
            }

            // Pass possible containers into function to choose some
            var chosenContainerIds = GetContainersByProbability(key, data);
            foreach (var chosenContainerId in chosenContainerIds)
            {
                // Look up container object from full list of containers on map
                var containerObject = staticRandomisableContainersOnMap.FirstOrDefault(
                    staticContainer => staticContainer.Template.Id == chosenContainerId
                );
                if (containerObject is null)
                {
                    if(_logger.IsLogEnabled(LogLevel.Debug))
                    {
                        _logger.Debug(
                            $"Container: {chosenContainerId} not found in staticRandomisableContainersOnMap, this is bad"
                        );
                    }
                    continue;
                }

                // Add loot to container and push into result object
                var containerWithLoot = AddLootToContainer(
                    containerObject,
                    staticForcedOnMapClone,
                    staticLootDist.Value,
                    staticAmmoDist,
                    locationId
                );
                result.Add(containerWithLoot.Template);
                staticContainerCount++;

                staticLootItemCount += containerWithLoot.Template.Items.Count;
            }
        }

        _logger.Success($"A total of {staticLootItemCount} static items spawned");

        _logger.Success(
            _localisationService.GetText("location-containers_generated_success", staticContainerCount)
        );

        return result;
    }

    /// <summary>
    ///     Get containers with a non-100% chance to spawn OR are NOT on the container type randomistion blacklist
    /// </summary>
    /// <param name="staticContainers"></param>
    /// <returns>StaticContainerData array</returns>
    protected List<StaticContainerData> GetRandomisableContainersOnMap(List<StaticContainerData> staticContainers)
    {
        return staticContainers.Where(
                staticContainer =>
                    staticContainer.Probability != 1 &&
                    !staticContainer.Template.IsAlwaysSpawn.GetValueOrDefault(false) &&
                    !_locationConfig.ContainerRandomisationSettings.ContainerTypesToNotRandomise.Contains(
                        staticContainer.Template.Items[0].Template
                    )
            )
            .ToList();
    }

    /// <summary>
    ///     Get containers with 100% spawn rate or have a type on the randomistion ignore list
    /// </summary>
    /// <param name="staticContainersOnMap"></param>
    /// <returns>IStaticContainerData array</returns>
    protected List<StaticContainerData> GetGuaranteedContainers(List<StaticContainerData> staticContainersOnMap)
    {
        return staticContainersOnMap.Where(
                staticContainer =>
                    staticContainer.Probability == 1 ||
                    staticContainer.Template.IsAlwaysSpawn.GetValueOrDefault(false) ||
                    _locationConfig.ContainerRandomisationSettings.ContainerTypesToNotRandomise.Contains(
                        staticContainer.Template.Items[0].Template
                    )
            )
            .ToList();
    }

    /// <summary>
    ///     Choose a number of containers based on their probabilty value to fulfil the desired count in
    ///     containerData.chosenCount
    /// </summary>
    /// <param name="groupId">Name of the group the containers are being collected for</param>
    /// <param name="containerData">Containers and probability values for a groupId</param>
    /// <returns>List of chosen container Ids</returns>
    protected List<string> GetContainersByProbability(string groupId, ContainerGroupCount containerData)
    {
        var chosenContainerIds = new List<string>();

        var containerIds = containerData.ContainerIdsWithProbability.Keys.ToList();
        if (containerData.ChosenCount > containerIds.Count)
        {
            if(_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug(
                    $"Group: {groupId} wants {containerData.ChosenCount} containers but pool only has {containerIds.Count}, add what's available"
                );
            }
            return containerIds;
        }

        // Create probability array with all possible container ids in this group and their relative probability of spawning
        var containerDistribution =
            new ProbabilityObjectArray<ProbabilityObject<string, double>, string, double>(_mathUtil, _cloner);
        foreach (var x in containerIds)
        {
            var value = containerData.ContainerIdsWithProbability[x];
            containerDistribution.Add(new ProbabilityObject<string, double>(x, value, value));
        }

        chosenContainerIds.AddRange(containerDistribution.Draw((int)containerData.ChosenCount));

        return chosenContainerIds;
    }

    /// <summary>
    ///     Get a mapping of each groupid and the containers in that group + count of containers to spawn on map
    /// </summary>
    /// <param name="containersGroups">Container group values</param>
    /// <returns>dictionary keyed by groupId</returns>
    protected Dictionary<string, ContainerGroupCount> GetGroupIdToContainerMappings(
        StaticContainer staticContainerGroupData,
        List<StaticContainerData> staticContainersOnMap)
    {
        // Create dictionary of all group ids and choose a count of containers the map will spawn of that group
        var mapping = new Dictionary<string, ContainerGroupCount>();
        foreach (var groupKvP in staticContainerGroupData.ContainersGroups)
        {
            if (staticContainerGroupData.ContainersGroups.TryGetValue(groupKvP.Key, out var groupData))
            {
                mapping[groupKvP.Key] = new ContainerGroupCount
                {
                    ContainerIdsWithProbability = new Dictionary<string, double>(),
                    ChosenCount = _randomUtil.GetInt(
                        (int)Math.Round(
                            groupData.MinContainers.Value *
                            _locationConfig.ContainerRandomisationSettings.ContainerGroupMinSizeMultiplier
                        ),
                        (int)Math.Round(
                            groupData.MaxContainers.Value *
                            _locationConfig.ContainerRandomisationSettings.ContainerGroupMaxSizeMultiplier
                        )
                    )
                };
            }
        }

        // Add an empty group for containers without a group id but still have a < 100% chance to spawn
        // Likely bad BSG data, will be fixed...eventually, example of the groupids: `NEED_TO_BE_FIXED1`,`NEED_TO_BE_FIXED_SE02`, `NEED_TO_BE_FIXED_NW_01`
        mapping[""] = new ContainerGroupCount { ContainerIdsWithProbability = new Dictionary<string, double>(), ChosenCount = -1 };

        // Iterate over all containers and add to group keyed by groupId
        // Containers without a group go into a group with empty key ""
        foreach (var container in staticContainersOnMap)
        {
            if (!staticContainerGroupData.Containers.TryGetValue(container.Template.Id, out var groupData))
            {
                _logger.Error(
                    _localisationService.GetText(
                        "location-unable_to_find_container_in_statics_json",
                        container.Template.Id
                    )
                );

                continue;
            }

            if (container.Probability >= 1)
            {
                if(_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"Container {container.Template.Id} with group: {groupData.GroupId} had 100 % chance to spawn was picked as random container, skipping"
                    );
                }

                continue;
            }

            mapping.TryAdd(groupData.GroupId, new ContainerGroupCount
            {
                ChosenCount = 0d,
                ContainerIdsWithProbability = new Dictionary<string, double>()
            });
            mapping[groupData.GroupId].ContainerIdsWithProbability.TryAdd(container.Template.Id, container.Probability.Value);
        }

        return mapping;
    }

    /// <summary>
    ///     Choose loot to put into a static container based on weighting
    ///     Handle forced items + seasonal item removal when not in season
    /// </summary>
    /// <param name="staticContainer">The container itself we will add loot to</param>
    /// <param name="staticForced">Loot we need to force into the container</param>
    /// <param name="staticLootDist">staticLoot.json</param>
    /// <param name="staticAmmoDist">staticAmmo.json</param>
    /// <param name="locationName">Name of the map to generate static loot for</param>
    /// <returns>StaticContainerData</returns>
    protected StaticContainerData AddLootToContainer(StaticContainerData staticContainer,
        List<StaticForced>? staticForced,
        Dictionary<string, StaticLootDetails> staticLootDist,
        Dictionary<string, List<StaticAmmoDetails>> staticAmmoDist, string locationName
    )
    {
        var containerClone = _cloner.Clone(staticContainer);
        var containerTpl = containerClone.Template.Items[0].Template;

        // Create new unique parent id to prevent any collisions
        var parentId = _hashUtil.Generate();
        containerClone.Template.Root = parentId;
        containerClone.Template.Items[0].Id = parentId;

        var containerMap = GetContainerMapping(containerTpl);

        // Choose count of items to add to container
        var itemCountToAdd = GetWeightedCountOfContainerItems(containerTpl, staticLootDist, locationName);

        // Get all possible loot items for container
        var containerLootPool = GetPossibleLootItemsForContainer(containerTpl, staticLootDist);

        // Some containers need to have items forced into it (quest keys etc)
        var tplsForced = staticForced
            .Where((forcedStaticProp) => forcedStaticProp.ContainerId == containerClone.Template.Id)
            .Select((x) => x.ItemTpl);

        // Draw random loot
        // Allow money to spawn more than once in container
        var failedToFitCount = 0;
        var locklist = _itemHelper.GetMoneyTpls();

        // Choose items to add to container, factor in weighting + lock money down
        // Filter out items picked that're already in the above `tplsForced` array
        var chosenTpls = containerLootPool
            .Draw(itemCountToAdd, _locationConfig.AllowDuplicateItemsInStaticContainers, locklist)
            .Where((tpl) => !tplsForced.Contains(tpl));

        // Add forced loot to chosen item pool
        var tplsToAddToContainer = tplsForced.Concat(chosenTpls);
        foreach (var tplToAdd in tplsToAddToContainer)
        {
            var chosenItemWithChildren = CreateStaticLootItem(tplToAdd, staticAmmoDist, parentId);
            if (chosenItemWithChildren is null)
            {
                continue;
            }

            var items = _locationConfig.TplsToStripChildItemsFrom.Contains(tplToAdd)
                ? [chosenItemWithChildren.Items[0]] // Strip children from parent
                : chosenItemWithChildren.Items;
            var itemSize = GetItemSize(items);
            var width = (int)itemSize.Width;
            var height = (int)itemSize.Height;

            // look for open slot to put chosen item into
            var result = _containerHelper.FindSlotForItem(containerMap, width, height);
            if (!result.Success.GetValueOrDefault(false))
            {
                if (failedToFitCount > _locationConfig.FitLootIntoContainerAttempts)
                {
                    // x attempts to fit an item, container is probably full, stop trying to add more
                    break;
                }

                // Can't fit item, skip
                failedToFitCount++;

                continue;
            }
            
            _containerHelper.FillContainerMapWithItem(
                containerMap,
                result.X.Value,
                result.Y.Value,
                width,
                height,
                result.Rotation.GetValueOrDefault(false)
            );

            var rotation = result.Rotation.GetValueOrDefault(false) ? 1 : 0;
             
            items[0].SlotId = "main";
            items[0].Location = new ItemLocation{ X = result.X, Y = result.Y, R = rotation };

            // Add loot to container before returning
            containerClone.Template.Items.AddRange(items);
        }

        return containerClone;
    }

    protected ItemSize? GetItemSize(List<Item>? items)
    {
        var rootItem = items[0];
        if (items.Count == 1)
        {
            var itemTemplate = _itemHelper.GetItem(rootItem.Template).Value;
            if (itemTemplate.Properties is null)
            {
                _logger.Error($"Unable to process item: {rootItem.Template}. it lacks _props");

                return null;
            }

            // Single item, get items properties
            return new ItemSize
            {
                Width = itemTemplate.Properties.Width.Value,
                Height = itemTemplate.Properties.Height.Value
            };
        }

        
        // Multi-mod-item, use helper to get size of item + attached mods
        var result = _inventoryHelper.GetItemSize(rootItem.Template, rootItem.Id, items);
        return new ItemSize
        {
            Width = result[0],
            Height = result[1]
        };

    }

    /// <summary>
    ///     Get a 2D grid of a container's item slots
    /// </summary>
    /// <param name="containerTpl">Tpl id of the container</param>
    protected int[][] GetContainerMapping(string containerTpl)
    {
        // Get template from db
        var containerTemplate = _itemHelper.GetItem(containerTpl).Value;

        // Get height/width
        var height = containerTemplate.Properties.Grids[0].Props.CellsV;
        var width = containerTemplate.Properties.Grids[0].Props.CellsH;

        return _inventoryHelper.GetBlankContainerMap(height.Value, width.Value);
    }

    /// <summary>
    ///     Look up a containers itemcountDistribution data and choose an item count based on the found weights
    /// </summary>
    /// <param name="containerTypeId">Container to get item count for</param>
    /// <param name="staticLootDist">staticLoot.json</param>
    /// <param name="locationName">Map name (to get per-map multiplier for from config)</param>
    /// <returns>item count</returns>
    protected int GetWeightedCountOfContainerItems(string containerTypeId,
        Dictionary<string, StaticLootDetails> staticLootDist, string locationName)
    {
        // Create probability array to calcualte the total count of lootable items inside container
        var itemCountArray =
            new ProbabilityObjectArray<ProbabilityObject<int, float?>, int, float?>(_mathUtil, _cloner);
        var countDistribution = staticLootDist[containerTypeId]?.ItemCountDistribution;
        if (countDistribution is null)
        {
            _logger.Warning(
                _localisationService.GetText(
                    "location-unable_to_find_count_distribution_for_container",
                    new
                    {
                        containerId = containerTypeId, locationName
                    }
                )
            );

            return 0;
        }

        foreach (var itemCountDistribution in countDistribution)
            // Add each count of items into array
            itemCountArray.Add(
                new ProbabilityObject<int, float?>(
                    itemCountDistribution.Count.Value,
                    itemCountDistribution.RelativeProbability.Value,
                    null
                )
            );

        return (int)Math.Round(GetStaticLootMultiplierForLocation(locationName) * itemCountArray.Draw()[0]);
    }

    /// <summary>
    ///     Get all possible loot items that can be placed into a container
    ///     Do not add seasonal items if found + current date is inside seasonal event
    /// </summary>
    /// <param name="containerTypeId">Container to get possible loot for</param>
    /// <param name="staticLootDist">staticLoot.json</param>
    /// <returns>ProbabilityObjectArray of item tpls + probabilty</returns>
    protected ProbabilityObjectArray<ProbabilityObject<string, float?>, string, float?> GetPossibleLootItemsForContainer(string containerTypeId,
        Dictionary<string, StaticLootDetails> staticLootDist) // TODO: Type Fuckery, return type was ProbabilityObjectArray<string, number>
    {
        var seasonalEventActive = _seasonalEventService.SeasonalEventEnabled();
        var seasonalItemTplBlacklist = _seasonalEventService.GetInactiveSeasonalEventItems();

        var itemDistribution =
            new ProbabilityObjectArray<ProbabilityObject<string, float?>, string, float?>(_mathUtil, _cloner);

        var itemContainerDistribution = staticLootDist[containerTypeId]?.ItemDistribution;
        if (itemContainerDistribution is null)
        {
            _logger.Warning(_localisationService.GetText("location-missing_item_distribution_data", containerTypeId));

            return itemDistribution;
        }

        foreach (var icd in itemContainerDistribution)
        {
            if (!seasonalEventActive && seasonalItemTplBlacklist.Contains(icd.Tpl))
            {
                // Skip seasonal event items if they're not enabled
                continue;
            }

            // Ensure no blacklisted lootable items are in pool
            if (_itemFilterService.IsLootableItemBlacklisted(icd.Tpl))
            {
                continue;
            }

            itemDistribution.Add(new ProbabilityObject<string, float?>(icd.Tpl, icd.RelativeProbability.Value, null));
        }

        return itemDistribution;
    }

    protected double GetLooseLootMultiplierForLocation(string location)
    {
        return _locationConfig.LooseLootMultiplier[location];
    }

    protected double GetStaticLootMultiplierForLocation(string location)
    {
        return _locationConfig.StaticLootMultiplier[location];
    }

    /// <summary>
    ///     Create array of loose + forced loot using probability system
    /// </summary>
    /// <param name="dynamicLootDist"></param>
    /// <param name="staticAmmoDist"></param>
    /// <param name="locationName">Location to generate loot for</param>
    /// <returns>Array of spawn points with loot in them</returns>
    public List<SpawnpointTemplate> GenerateDynamicLoot(LooseLoot dynamicLootDist,
        Dictionary<string, List<StaticAmmoDetails>> staticAmmoDist,
        string locationName)
    {
        List<SpawnpointTemplate> loot = [];
        List<Spawnpoint> dynamicForcedSpawnPoints = [];

        // Remove christmas items from loot data
        if (!_seasonalEventService.ChristmasEventEnabled())
        {
            dynamicLootDist.Spawnpoints = dynamicLootDist.Spawnpoints.Where(
                    (point) => !point.Template.Id.StartsWith("christmas")
                )
                .ToList();
            dynamicLootDist.SpawnpointsForced = dynamicLootDist.SpawnpointsForced.Where(
                    (point) => !point.Template.Id.StartsWith("christmas")
                )
                .ToList();
        }

        // Build the list of forced loot from both `spawnpointsForced` and any point marked `IsAlwaysSpawn`
        dynamicForcedSpawnPoints.AddRange(dynamicLootDist.SpawnpointsForced);
        dynamicForcedSpawnPoints.AddRange(dynamicLootDist.Spawnpoints.Where((point) => point.Template.IsAlwaysSpawn ?? false));

        // Add forced loot
        AddForcedLoot(loot, dynamicForcedSpawnPoints, locationName, staticAmmoDist);

        var allDynamicSpawnpoints = dynamicLootDist.Spawnpoints;

        // Draw from random distribution
        var desiredSpawnpointCount = Math.Round(
            GetLooseLootMultiplerForLocation(locationName) *
            _randomUtil.GetNormallyDistributedRandomNumber(
                (double)dynamicLootDist.SpawnpointCount.Mean,
                (double)dynamicLootDist.SpawnpointCount.Std
            )
        );

        // Positions not in forced but have 100% chance to spawn
        List<Spawnpoint> guaranteedLoosePoints = [];

        var blacklistedSpawnpoints = _locationConfig.LooseLootBlacklist.GetValueOrDefault(locationName);
        var spawnpointArray = new ProbabilityObjectArray<ProbabilityObject<string, Spawnpoint>, string, Spawnpoint>(_mathUtil, _cloner, []);

        foreach (var spawnpoint in allDynamicSpawnpoints)
        {
            // Point is blacklisted, skip
            if (blacklistedSpawnpoints?.Contains(spawnpoint.Template.Id) ?? false)
            {
                if(_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"Ignoring loose loot location: {spawnpoint.Template.Id}");
                }
                continue;
            }

            // We've handled IsAlwaysSpawn above, so skip them
            if (spawnpoint.Template.IsAlwaysSpawn ?? false)
            {
                continue;
            }

            // 100%, add it to guaranteed
            if (spawnpoint.Probability == 1)
            {
                guaranteedLoosePoints.Add(spawnpoint);
                continue;
            }

            spawnpointArray.Add(new ProbabilityObject<string, Spawnpoint>(spawnpoint.Template.Id, spawnpoint.Probability ?? 0, spawnpoint));
        }

        // Select a number of spawn points to add loot to
        // Add ALL loose loot with 100% chance to pool
        List<Spawnpoint> chosenSpawnpoints = [];
        chosenSpawnpoints.AddRange(guaranteedLoosePoints);

        var randomSpawnpointCount = desiredSpawnpointCount - chosenSpawnpoints.Count;
        // Only draw random spawn points if needed
        if (randomSpawnpointCount > 0 && spawnpointArray.Count > 0)
        {
            // Add randomly chosen spawn points
            foreach (var si in spawnpointArray.Draw((int)randomSpawnpointCount, false))
            {
                chosenSpawnpoints.Add(spawnpointArray.Data(si));
            }
        }

        // Filter out duplicate locationIds // prob can be done better
        chosenSpawnpoints = chosenSpawnpoints.GroupBy(spawnpoint => spawnpoint.LocationId).Select(group => group.First()).ToList();

        // Do we have enough items in pool to fulfill requirement
        var tooManySpawnPointsRequested = desiredSpawnpointCount - chosenSpawnpoints.Count > 0;
        if (tooManySpawnPointsRequested)
        {
            if(_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug(
                    _localisationService.GetText(
                        "location-spawn_point_count_requested_vs_found",
                        new
                        {
                            requested = desiredSpawnpointCount + guaranteedLoosePoints.Count,
                            found = chosenSpawnpoints.Count,
                            mapName = locationName,
                        }
                    )
                );
            }
        }

        // Iterate over spawnpoints
        var seasonalEventActive = _seasonalEventService.SeasonalEventEnabled();
        var seasonalItemTplBlacklist = _seasonalEventService.GetInactiveSeasonalEventItems();
        foreach (var spawnPoint in chosenSpawnpoints)
        {
            // Spawnpoint is invalid, skip it
            if (spawnPoint.Template is null)
            {
                _logger.Warning(
                    _localisationService.GetText("location-missing_dynamic_template", spawnPoint.LocationId)
                );

                continue;
            }

            // Ensure no blacklisted lootable items are in pool
            spawnPoint.Template.Items = spawnPoint.Template.Items.Where(
                    (item) => !_itemFilterService.IsLootableItemBlacklisted(item.Template)
                )
                .ToList();

            // Ensure no seasonal items are in pool if not in-season
            if (!seasonalEventActive)
            {
                spawnPoint.Template.Items = spawnPoint.Template.Items.Where(
                        (item) => !seasonalItemTplBlacklist.Contains(item.Template)
                    )
                    .ToList();
            }

            // Spawn point has no items after filtering, skip
            if (spawnPoint.Template.Items is null || spawnPoint.Template.Items.Count == 0)
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        _localisationService.GetText("location-spawnpoint_missing_items", spawnPoint.Template.Id)
                    );
                }

                continue;
            }

            // Get an array of allowed IDs after above filtering has occured
            var validItemIds = spawnPoint.Template.Items.Select((item) => item.Id).ToList();

            // Construct container to hold above filtered items, letting us pick an item for the spot
            var itemArray = new ProbabilityObjectArray<ProbabilityObject<string, double?>, string, double?>(_mathUtil, _cloner, []);
            foreach (var itemDist in spawnPoint.ItemDistribution)
            {
                if (!validItemIds.Contains(itemDist.ComposedKey.Key))
                {
                    continue;
                }

                itemArray.Add(new ProbabilityObject<string, double?>(itemDist.ComposedKey.Key, itemDist.RelativeProbability ?? 0, null));
            }

            if (itemArray.Count == 0)
            {
                _logger.Warning(
                    _localisationService.GetText("location-loot_pool_is_empty_skipping", spawnPoint.Template.Id)
                );

                continue;
            }

            // Draw a random item from spawn points possible items
            var chosenComposedKey = itemArray.Draw(1).FirstOrDefault();
            var createItemResult = CreateDynamicLootItem(
                chosenComposedKey,
                spawnPoint.Template.Items,
                staticAmmoDist
            );

            // Root id can change when generating a weapon, ensure ids match
            spawnPoint.Template.Root = createItemResult.Items.FirstOrDefault().Id;

            // Overwrite entire pool with chosen item
            spawnPoint.Template.Items = createItemResult.Items;

            loot.Add(spawnPoint.Template);
        }

        return loot;
    }

    /// <summary>
    ///     Add forced spawn point loot into loot parameter list
    /// </summary>
    /// <param name="lootLocationTemplates">List to add forced loot spawn locations to</param>
    /// <param name="forcedSpawnPoints">Forced loot locations that must be added</param>
    /// <param name="locationName">Name of map currently having force loot created for</param>
    protected void AddForcedLoot(List<SpawnpointTemplate> lootLocationTemplates,
        List<Spawnpoint> forcedSpawnPoints, string locationName,
        Dictionary<string, List<StaticAmmoDetails>> staticAmmoDist)
    {
        var lootToForceSingleAmountOnMap = _locationConfig.ForcedLootSingleSpawnById.GetValueOrDefault(locationName);
        if (lootToForceSingleAmountOnMap is not null)
        {
            // Process loot items defined as requiring only 1 spawn position as they appear in multiple positions on the map
            foreach (var itemTpl in lootToForceSingleAmountOnMap)
            {
                // Get all spawn positions for item tpl in forced loot array
                var items = forcedSpawnPoints.Where(
                    (forcedSpawnPoint) => forcedSpawnPoint.Template.Items[0].Template == itemTpl
                );
                if (items is null || !items.Any())
                {
                    if(_logger.IsLogEnabled(LogLevel.Debug))
                    {
                        _logger.Debug($"Unable to adjust loot item {itemTpl} as it does not exist inside {locationName} forced loot.");
                    }
                    continue;
                }

                // Create probability array of all spawn positions for this spawn id
                var spawnpointArray = new ProbabilityObjectArray<ProbabilityObject<string, Spawnpoint>, string, Spawnpoint>(_mathUtil, _cloner, []);
                foreach (var si in items)
                {
                    // use locationId as template.Id is the same across all items
                    spawnpointArray.Add(new ProbabilityObject<string, Spawnpoint>(si.LocationId, si.Probability ?? 0, si));
                }

                // Choose 1 out of all found spawn positions for spawn id and add to loot array
                foreach (var spawnPointLocationId in spawnpointArray.Draw(1, false))
                {
                    var itemToAdd = items.FirstOrDefault((item) => item.LocationId == spawnPointLocationId);
                    var lootItem = itemToAdd?.Template;
                    if (lootItem is null)
                    {
                        _logger.Warning($"Item with spawn point id {spawnPointLocationId} could not be found, skipping");
                        continue;
                    }

                    var createItemResult = CreateDynamicLootItem(
                        lootItem.Items.FirstOrDefault().Id,
                        lootItem.Items,
                        staticAmmoDist
                    );

                    // Update root ID with the dynamically generated ID
                    lootItem.Root = createItemResult.Items.FirstOrDefault().Id;
                    lootItem.Items = createItemResult.Items;
                    lootLocationTemplates.Add(lootItem);
                }
            }
        }

        var seasonalEventActive = _seasonalEventService.SeasonalEventEnabled();
        var seasonalItemTplBlacklist = _seasonalEventService.GetInactiveSeasonalEventItems();

        // Add remaining forced loot to array
        foreach (var forcedLootLocation in forcedSpawnPoints)
        {
            var firstLootItemTpl = forcedLootLocation.Template.Items.FirstOrDefault().Template;

            // Skip spawn positions processed already
            if (lootToForceSingleAmountOnMap?.Contains(firstLootItemTpl) ?? false)
            {
                continue;
            }

            // Skip adding seasonal items when seasonal event is not active
            if (!seasonalEventActive && seasonalItemTplBlacklist.Contains(firstLootItemTpl))
            {
                continue;
            }

            var locationTemplateToAdd = forcedLootLocation.Template;
            var createItemResult = CreateDynamicLootItem(
                locationTemplateToAdd.Items.FirstOrDefault().Id,
                forcedLootLocation.Template.Items,
                staticAmmoDist
            );

            // Update root ID with the dynamically generated ID
            forcedLootLocation.Template.Root = createItemResult.Items.FirstOrDefault().Id;
            forcedLootLocation.Template.Items = createItemResult.Items;

            // Push forced location into array as long as it doesnt exist already
            var existingLocation = lootLocationTemplates.Any(
                (spawnPoint) => spawnPoint.Id == locationTemplateToAdd.Id
            );
            if (!existingLocation)
            {
                lootLocationTemplates.Add(locationTemplateToAdd);
            }
            else
            {
                if(_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"Attempted to add a forced loot location with Id: {locationTemplateToAdd.Id} to map {locationName} that already has that id in use, skipping"
                    );
                }
            }
        }
    }

    private ContainerItem CreateDynamicLootItem(string? chosenComposedKey, List<Item> items, Dictionary<string, List<StaticAmmoDetails>> staticAmmoDist)
    {
        var chosenItem = items.FirstOrDefault((item) => item.Id == chosenComposedKey);
        var chosenTpl = chosenItem?.Template;
        if (chosenTpl is null) {
            throw new Exception($"Item for tpl {chosenComposedKey} was not found in the spawn point");
        }
        var itemTemplate = _itemHelper.GetItem(chosenTpl).Value;
        if (itemTemplate is null) {
            _logger.Error($"Item tpl: {chosenTpl} cannot be found in database");
        }

        // Item array to return
        List<Item> itemWithMods = [];

        // Money/Ammo - don't rely on items in spawnPoint.template.Items so we can randomise it ourselves
        if (_itemHelper.IsOfBaseclasses(chosenTpl, [BaseClasses.MONEY, BaseClasses.AMMO])) {
            var stackCount =
                itemTemplate.Properties.StackMaxSize == 1
                    ? 1
                    : _randomUtil.GetInt((int)itemTemplate.Properties.StackMinRandom, (int)itemTemplate.Properties.StackMaxRandom);

            itemWithMods.Add(new Item {
                Id = _hashUtil.Generate(),
                Template = chosenTpl,
                Upd = new Upd { StackObjectsCount = stackCount }
            });
        } else if (_itemHelper.IsOfBaseclass(chosenTpl, BaseClasses.AMMO_BOX)) {
            // Fill with cartridges
            List<Item> ammoBoxItem = [ new Item { Id = _hashUtil.Generate(), Template = chosenTpl }];
            _itemHelper.AddCartridgesToAmmoBox(ammoBoxItem, itemTemplate);
            itemWithMods.AddRange(ammoBoxItem);
        } else if (_itemHelper.IsOfBaseclass(chosenTpl, BaseClasses.MAGAZINE)) {
            // Create array with just magazine
            List<Item> magazineItem = [new Item { Id = _hashUtil.Generate(), Template = chosenTpl }];

            if (_randomUtil.GetChance100(_locationConfig.StaticMagazineLootHasAmmoChancePercent)) {
                // Add randomised amount of cartridges
                _itemHelper.FillMagazineWithRandomCartridge(
                    magazineItem,
                    itemTemplate, // Magazine template
                    staticAmmoDist,
                    null,
                    _locationConfig.MinFillLooseMagazinePercent / 100
                );
            }

            itemWithMods.AddRange(magazineItem);
        } else {
            // Also used by armors to get child mods
            // Get item + children and add into array we return
            var itemWithChildren = _itemHelper.FindAndReturnChildrenAsItems(items, chosenItem.Id);

            // Ensure all IDs are unique
            itemWithChildren = _itemHelper.ReplaceIDs(_cloner.Clone(itemWithChildren));

            if (_locationConfig.TplsToStripChildItemsFrom.Contains(chosenItem.Template)) {
                // Strip children from parent before adding
                itemWithChildren = [itemWithChildren[0]];
            }

            itemWithMods.AddRange(itemWithChildren);
        }

        // Get inventory size of item
        var size = _itemHelper.GetItemSize(itemWithMods, itemWithMods[0].Id);

        return new ContainerItem { Items = itemWithMods, Width = size.Width, Height = size.Height };
    }

    private double GetLooseLootMultiplerForLocation(string location)
    {
        return _locationConfig.LooseLootMultiplier[location];
    }
    
    protected double getStaticLootMultiplerForLocation(string location) {
        return _locationConfig.StaticLootMultiplier[location];
    }


    // TODO: rewrite, BIG yikes
    protected ContainerItem? CreateStaticLootItem(
        string chosenTpl,
        Dictionary<string, List<StaticAmmoDetails>> staticAmmoDist,
        string? parentId = null)
    {
        var itemTemplate = _itemHelper.GetItem(chosenTpl).Value;
        if (itemTemplate.Properties is null)
        {
            _logger.Error($"Unable to process item: {chosenTpl}. it lacks _props");

            return null;
        }

        var width = itemTemplate.Properties.Width;
        var height = itemTemplate.Properties.Height;
        List<Item> items = [new Item { Id = _hashUtil.Generate(), Template = chosenTpl }];
        var rootItem = items.FirstOrDefault();

        // Use passed in parentId as override for new item
        if (!string.IsNullOrEmpty(parentId))
        {
            rootItem.ParentId = parentId;
        }

        if (
            _itemHelper.IsOfBaseclass(chosenTpl, BaseClasses.MONEY) ||
            _itemHelper.IsOfBaseclass(chosenTpl, BaseClasses.AMMO)
        )
        {
            // Edge case - some ammos e.g. flares or M406 grenades shouldn't be stacked
            var stackCount = itemTemplate.Properties.StackMaxSize == 1
                ? 1
                : _randomUtil.GetInt((int)(itemTemplate.Properties.StackMinRandom), (int)(itemTemplate.Properties.StackMaxRandom));

            rootItem.Upd = new Upd { StackObjectsCount = stackCount };
        }
        // No spawn point, use default template
        else if (_itemHelper.IsOfBaseclass(chosenTpl, BaseClasses.WEAPON))
        {
            List<Item> children = [];
            var defaultPreset = _cloner.Clone(_presetHelper.GetDefaultPreset(chosenTpl));
            if (defaultPreset?.Items is not null)
            {
                try
                {
                    children = _itemHelper.ReparentItemAndChildren(defaultPreset.Items[0], defaultPreset.Items);
                }
                catch (Exception e)
                {
                    // this item already broke it once without being reproducible tpl = "5839a40f24597726f856b511"; AKS-74UB Default
                    // 5ea03f7400685063ec28bfa8 // ppsh default
                    // 5ba26383d4351e00334c93d9 //mp7_devgru
                    _logger.Error(
                        _localisationService.GetText(
                            "location-preset_not_found",
                            new
                            {
                                tpl = chosenTpl,
                                defaultId = defaultPreset.Id,
                                defaultName = defaultPreset.Name,
                                parentId,
                            }
                        )
                    );

                    throw;
                }
            }
            else
            {
                // RSP30 (62178be9d0050232da3485d9/624c0b3340357b5f566e8766/6217726288ed9f0845317459) doesn't have any default presets and kills this code below as it has no chidren to reparent
                if(_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"createStaticLootItem() No preset found for weapon: {chosenTpl}");
                }
            }

            rootItem = items[0];
            if (rootItem is null)
            {
                _logger.Error(
                    _localisationService.GetText(
                        "location-missing_root_item",
                        new
                        {
                            tpl = chosenTpl,
                            parentId,
                        }
                    )
                );

                throw new Exception(_localisationService.GetText("location-critical_error_see_log"));
            }

            try
            {
                if (children?.Count > 0)
                {
                    items = _itemHelper.ReparentItemAndChildren(rootItem, children);
                }
            }
            catch (Exception e)
            {
                _logger.Error(
                    _localisationService.GetText(
                        "location-unable_to_reparent_item",
                        new
                        {
                            tpl = chosenTpl,
                            parentId = parentId,
                        }
                    )
                );

                throw;
            }

            // Here we should use generalized BotGenerators functions e.g. fillExistingMagazines in the future since
            // it can handle revolver ammo (it's not restructured to be used here yet.)
            // General: Make a WeaponController for Ragfair preset stuff and the generating weapons and ammo stuff from
            // BotGenerator
            var magazine = items.FirstOrDefault(item => item.SlotId == "mod_magazine");
            // some weapon presets come without magazine; only fill the mag if it exists
            if (magazine is not null)
            {
                var magTemplate = _itemHelper.GetItem(magazine.Template).Value;
                var weaponTemplate = _itemHelper.GetItem(chosenTpl).Value;

                // Create array with just magazine
                var defaultWeapon = _itemHelper.GetItem(rootItem.Template).Value;
                List<Item> magazineWithCartridges = [magazine];
                _itemHelper.FillMagazineWithRandomCartridge(
                    magazineWithCartridges,
                    magTemplate,
                    staticAmmoDist,
                    weaponTemplate.Properties.AmmoCaliber,
                    0.25,
                    defaultWeapon.Properties.DefAmmo,
                    defaultWeapon
                );

                // Replace existing magazine with above array
                items.Remove(magazine);
                items.AddRange(magazineWithCartridges);
            }

            var size = _itemHelper.GetItemSize(items, rootItem.Id);
            width = size.Width;
            height = size.Height;
        }
        // No spawnpoint to fall back on, generate manually
        else if (_itemHelper.IsOfBaseclass(chosenTpl, BaseClasses.AMMO_BOX))
        {
            _itemHelper.AddCartridgesToAmmoBox(items, itemTemplate);
        }
        else if (_itemHelper.IsOfBaseclass(chosenTpl, BaseClasses.MAGAZINE))
        {
            if (_randomUtil.GetChance100(_locationConfig.MagazineLootHasAmmoChancePercent))
            {
                // Create array with just magazine
                List<Item> magazineWithCartridges = [rootItem];
                _itemHelper.FillMagazineWithRandomCartridge(
                    magazineWithCartridges,
                    itemTemplate,
                    staticAmmoDist,
                    null,
                    _locationConfig.MinFillStaticMagazinePercent / 100
                );

                // Replace existing magazine with above array
                items.Remove(rootItem);
                items.AddRange(magazineWithCartridges);
            }
        }
        else if (_itemHelper.ArmorItemCanHoldMods(chosenTpl))
        {
            var defaultPreset = _presetHelper.GetDefaultPreset(chosenTpl);
            if (defaultPreset is not null)
            {
                List<Item> presetAndMods = _itemHelper.ReplaceIDs(_cloner.Clone(defaultPreset.Items));
                _itemHelper.RemapRootItemId(presetAndMods);

                // Use original items parentId otherwise item doesnt get added to container correctly
                presetAndMods[0].ParentId = rootItem.ParentId;
                items = presetAndMods;
            }
            else
            {
                // We make base item above, at start of function, no need to do it here
                if ((itemTemplate.Properties.Slots?.Count ?? 0) > 0)
                {
                    items = _itemHelper.AddChildSlotItems(
                        items,
                        itemTemplate,
                        _locationConfig.EquipmentLootSettings.ModSpawnChancePercent
                    );
                }
            }
        }

        return new ContainerItem { Items = items, Width = width, Height = height };
    }
}

public class ContainerGroupCount
{
    [JsonPropertyName("containerIdsWithProbability")]
    public Dictionary<string, double>? ContainerIdsWithProbability { get; set; }

    [JsonPropertyName("chosenCount")]
    public double? ChosenCount { get; set; }
}

public class ContainerItem
{
    [JsonPropertyName("items")]
    public List<Item>? Items { get; set; }

    [JsonPropertyName("width")]
    public double? Width { get; set; }

    [JsonPropertyName("height")]
    public double? Height { get; set; }
}
