using System.Collections.Frozen;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Repeatable;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Server.Core.Utils.Collections;
using SPTarkov.Server.Core.Utils.Json;
using BodyParts = SPTarkov.Server.Core.Constants.BodyParts;

namespace SPTarkov.Server.Core.Generators.RepeatableQuestGeneration;

// TODO: Refactor me!
[Injectable]
public class EliminationQuestGenerator(
    ISptLogger<EliminationQuestGenerator> logger,
    RandomUtil randomUtil,
    MathUtil mathUtil,
    RepeatableQuestHelper repeatableQuestHelper,
    ItemHelper itemHelper,
    RepeatableQuestRewardGenerator repeatableQuestRewardGenerator,
    DatabaseService databaseService,
    ServerLocalisationService localisationService,
    ConfigServer configServer,
    ICloner cloner
) : IRepeatableQuestGenerator
{
    /// <summary>
    /// Body parts to present to the client as opposed to the body part information in quest data.
    /// </summary>
    private static readonly FrozenDictionary<string, List<string>> _bodyPartsToClient = new Dictionary<string, List<string>>
    {
        { BodyParts.Arms, [BodyParts.LeftArm, BodyParts.RightArm] },
        { BodyParts.Legs, [BodyParts.LeftLeg, BodyParts.RightLeg] },
        { BodyParts.Head, [BodyParts.Head] },
        { BodyParts.Chest, [BodyParts.Chest, BodyParts.Stomach] },
    }.ToFrozenDictionary();

    /// <summary>
    /// MaxDistDifficulty is defined by 2, this could be a tuning parameter if we don't like the reward generation
    /// </summary>
    protected const int MaxDistDifficulty = 2;

    protected QuestConfig QuestConfig = configServer.GetConfig<QuestConfig>();

    protected record EliminationQuestGenerationData(
        EliminationConfig EliminationConfig,
        Dictionary<ELocationName, List<string>> LocationsConfig,
        ProbabilityObjectArray<string, BossInfo> TargetsConfig,
        ProbabilityObjectArray<string, List<string>> BodyPartsConfig,
        ProbabilityObjectArray<string, List<MongoId>> WeaponCategoryRequirementConfig,
        ProbabilityObjectArray<string, List<MongoId>> WeaponRequirementConfig
    );

    /// <summary>
    ///     Generate a randomised Elimination quest
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcLevel">Player's level for requested items and reward generation</param>
    /// <param name="traderId">Trader from which the quest will be provided</param>
    /// <param name="questTypePool">Pools for quests (used to avoid redundant quests)</param>
    /// <param name="repeatableConfig">
    ///     The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig
    ///     for the requested quest
    /// </param>
    /// <returns>Object of quest type format for "Elimination" (see assets/database/templates/repeatableQuests.json)</returns>
    public RepeatableQuest? Generate(
        MongoId sessionId,
        int pmcLevel,
        MongoId traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
    )
    {
        var generationData = GetGenerationData(repeatableConfig, pmcLevel);
        if (generationData is null)
        {
            logger.Error(localisationService.GetText("repeatable-eliminationQuestGenerationData-is-null"));
            return null;
        }

        // the difficulty of the quest varies in difficulty depending on the condition
        // possible conditions are
        // - amount of npcs to kill
        // - type of npc to kill (scav, boss, pmc)
        // - with hit to what body part they should be killed
        // - from what distance they should be killed
        // a random combination of listed conditions can be required
        // possible conditions elements and their relative probability can be defined in QuestConfig.js
        // We use ProbabilityObjectArray to draw by relative probability. e.g. for targets:
        // "targets": {
        //    "Savage": 7,
        //    "AnyPmc": 2,
        //    "bossBully": 0.5
        // }
        // higher is more likely. We define the difficulty to be the inverse of the relative probability.

        // We want to generate a reward which is scaled by the difficulty of this mission. To get an upper bound with which we scale
        // the actual difficulty we calculate the minimum and maximum difficulty (max being the sum of max of each condition type
        // times the number of kills we have to perform):

        // The minimum difficulty is the difficulty for the most probable (= easiest target) with no additional conditions
        var minDifficulty = 1 / generationData.TargetsConfig.MaxProbability(); // min difficulty is the lowest amount of scavs without any constraints

        // Target on bodyPart max. difficulty is that of the least probable element
        var maxTargetDifficulty = 1 / generationData.TargetsConfig.MinProbability();
        var maxBodyPartsDifficulty = generationData.EliminationConfig.MinKills / generationData.BodyPartsConfig.MinProbability();

        var maxKillDifficulty = generationData.EliminationConfig.MaxKills;
        var targetPool = questTypePool.Pool.Elimination;

        // Get a random bot type to eliminate
        var (botTypeToEliminate, targetsConfig) = GetBotTypeToEliminate(generationData, questTypePool);
        if (botTypeToEliminate is null || targetsConfig is null)
        {
            logger.Warning(localisationService.GetText("repeatable-no-bot-types-remain"));
            return null;
        }

        var targetDifficulty = 1 / targetsConfig.Probability(botTypeToEliminate);

        if (targetPool.Targets is null)
        {
            logger.Error(localisationService.GetText("repeatable-unable-targets-are-null"));
            return null;
        }

        // Try and get a target location pool for this bot type
        if (!targetPool.Targets.TryGetValue(botTypeToEliminate, out var targetLocationPool))
        {
            logger.Error(localisationService.GetText("repeatable-unable-get-target-pool", botTypeToEliminate));

            return null;
        }

        // Try and get a location key for this quest
        if (
            !TryGetLocationKey(generationData, targetPool, botTypeToEliminate, targetLocationPool.Locations, out var locationKey)
            || locationKey is null
        )
        {
            logger.Error(localisationService.GetText("repeatable-unable-get-location-key", botTypeToEliminate));

            return null;
        }

        // Generate a body part, make sure we ref the body part difficulty so it can be adjusted
        var bodyPartsToClient = new List<string>();
        var bodyPartDifficulty = 0d;
        var generateBodyParts = randomUtil.GetChance100(generationData.EliminationConfig.BodyPartChance);
        if (generateBodyParts)
        {
            // draw the target body part and calculate the difficulty factor
            bodyPartsToClient.AddRange(GenerateBodyParts(generationData, ref bodyPartDifficulty));
        }

        // Draw a distance condition
        var isDistanceRequirementAllowed = IsDistanceRequirementAllowed(generationData, botTypeToEliminate, locationKey, targetsConfig);

        int? distance = null;
        var distanceDifficulty = 0;

        // Generate a distance requirement
        if (isDistanceRequirementAllowed)
        {
            var (dist, distDiff) = GenerateDistanceRequirement(generationData);
            distance = dist;
            distanceDifficulty = distDiff;
        }

        string? allowedWeaponsCategory = null;

        var generateWeaponCategoryRequirement = randomUtil.GetChance100(generationData.EliminationConfig.WeaponCategoryRequirementChance);

        // Generate a weapon category requirement
        if (generateWeaponCategoryRequirement)
        {
            allowedWeaponsCategory = GenerateWeaponCategoryRequirement(generationData, distance);
        }

        // Only allow a specific weapon requirement if a weapon category was not chosen
        MongoId? allowedWeapon = null;

        var generateWeaponRequirement = randomUtil.GetChance100(generationData.EliminationConfig.WeaponRequirementChance);

        // Generate a weapon requirement
        if (!generateWeaponCategoryRequirement && generateWeaponRequirement)
        {
            allowedWeapon = GenerateSpecificWeaponRequirement(generationData);
        }

        // Draw how many npm kills are required
        var desiredKillCount = GetEliminationKillCount(botTypeToEliminate, targetsConfig, generationData.EliminationConfig);

        var killDifficulty = desiredKillCount;

        // not perfectly happy here; we give difficulty = 1 to the quest reward generation when we have the most difficult mission
        // e.g. killing reshala 5 times from a distance of 200m with a headshot.
        var maxDifficulty = DifficultyWeighing(1, 1, 1, 1, 1);
        var curDifficulty = DifficultyWeighing(
            targetDifficulty.Value / maxTargetDifficulty,
            bodyPartDifficulty / maxBodyPartsDifficulty,
            distanceDifficulty / MaxDistDifficulty,
            killDifficulty / maxKillDifficulty,
            allowedWeaponsCategory is not null || allowedWeapon is not null ? 1 : 0
        );

        // Aforementioned issue makes it a bit crazy since now all easier quests give significantly lower rewards than Completion / Exploration
        // I therefore moved the mapping a bit up (from 0.2...1 to 0.5...2) so that normal difficulty still gives good reward and having the
        // crazy maximum difficulty will lead to a higher difficulty reward gain factor than 1
        var difficulty = mathUtil.MapToRange(curDifficulty, minDifficulty, maxDifficulty, 0.5, 2);

        var quest = repeatableQuestHelper.GenerateRepeatableTemplate(
            RepeatableQuestType.Elimination,
            traderId,
            repeatableConfig.Side,
            sessionId
        );

        if (quest is null)
        {
            logger.Error(localisationService.GetText("repeatable-quest_generation_failed_no_template", "elimination"));

            return null;
        }

        // ASSUMPTION: All fence quests are for scavs
        if (traderId == Traders.FENCE)
        {
            quest.Side = "Scav";
        }

        var availableForFinishCondition = quest.Conditions.AvailableForFinish![0];
        availableForFinishCondition.Counter!.Id = new MongoId();
        availableForFinishCondition.Counter.Conditions = [];

        // Only add specific location condition if specific map selected
        if (locationKey != "any")
        {
            var locationId = Enum.Parse<ELocationName>(locationKey);
            availableForFinishCondition.Counter.Conditions.Add(GenerateEliminationLocation(generationData.LocationsConfig[locationId]));
        }

        availableForFinishCondition.Counter.Conditions.Add(
            GenerateEliminationCondition(botTypeToEliminate, bodyPartsToClient, distance, allowedWeapon, allowedWeaponsCategory)
        );
        availableForFinishCondition.Value = desiredKillCount;
        availableForFinishCondition.Id = new MongoId();

        // Get the quest location, default to any if none exist
        quest.Location = repeatableQuestHelper.GetQuestLocationByMapId(locationKey) ?? "any";

        quest.Rewards = repeatableQuestRewardGenerator.GenerateReward(
            pmcLevel,
            Math.Min(difficulty, 1),
            traderId,
            repeatableConfig,
            generationData.EliminationConfig
        );

        return quest;
    }

    protected EliminationQuestGenerationData? GetGenerationData(RepeatableQuestConfig repeatableConfig, int pmcLevel)
    {
        var eliminationConfig = repeatableQuestHelper.GetEliminationConfigByPmcLevel(pmcLevel, repeatableConfig);

        if (eliminationConfig is null)
        {
            logger.Error(localisationService.GetText("repeatable-elimination-config-not-found"));
            return null;
        }

        var locationsConfig = repeatableConfig.Locations;

        var targetsConfig = new ProbabilityObjectArray<string, BossInfo>(cloner, eliminationConfig.Targets);
        var bodyPartsConfig = new ProbabilityObjectArray<string, List<string>>(cloner, eliminationConfig.BodyParts);
        var weaponCategoryRequirementConfig = new ProbabilityObjectArray<string, List<MongoId>>(
            cloner,
            eliminationConfig.WeaponCategoryRequirements
        );
        var weaponRequirementConfig = new ProbabilityObjectArray<string, List<MongoId>>(cloner, eliminationConfig.WeaponRequirements);

        return new EliminationQuestGenerationData(
            eliminationConfig,
            locationsConfig,
            targetsConfig,
            bodyPartsConfig,
            weaponCategoryRequirementConfig,
            weaponRequirementConfig
        );
    }

    /// <summary>
    ///     Gets and filters a bot type for this elimination quest
    /// </summary>
    /// <param name="generationData">Generation data</param>
    /// <param name="questTypePool">Quest pool to generate from</param>
    /// <returns>target, filtered targets config</returns>
    protected (string?, ProbabilityObjectArray<string, BossInfo>?) GetBotTypeToEliminate(
        EliminationQuestGenerationData generationData,
        QuestTypePool questTypePool
    )
    {
        var targetPool = questTypePool.Pool.Elimination;

        var targetsConfig = generationData.TargetsConfig.Filter(x => targetPool.Targets.ContainsKey(x.Key));

        if (targetsConfig.Count != 0 && !targetsConfig.All(x => x.Data?.IsBoss ?? false))
        {
            return (targetsConfig.Draw()[0], targetsConfig);
        }

        // There are no more targets left for elimination; delete it as a possible quest type
        // also if only bosses are left we need to leave otherwise it's a guaranteed boss elimination
        // -> then it would not be a quest with low probability anymore
        questTypePool.Types = questTypePool.Types.Where(t => t != "Elimination").ToList();
        return (null, null);
    }

    /// <summary>
    ///     Try and get a location key to generate this quest for
    /// </summary>
    /// <param name="generationData">Generation data</param>
    /// <param name="targetPool">Target pool</param>
    /// <param name="botTypeToEliminate">Bot type to eliminate</param>
    /// <param name="locations">locations to choose from</param>
    /// <param name="locationKey">selected location key</param>
    /// <returns>True if location key selected, false otherwise</returns>
    protected bool TryGetLocationKey(
        EliminationQuestGenerationData generationData,
        EliminationPool targetPool,
        string botTypeToEliminate,
        List<string> locations,
        out string? locationKey
    )
    {
        var useSpecificLocation = randomUtil.GetChance100(generationData.EliminationConfig.SpecificLocationChance);

        switch (useSpecificLocation)
        {
            // We're not using a specific location, and the locations contain any.
            case false when locations.Contains("any"):
                locationKey = "any";
                return true;
            // We're not using a specific location and locations didn't contain any.
            case false:
                logger.Error(localisationService.GetText("repeatable-elimination-any-not-found"));
                locationKey = null;
                return false;
        }

        // Don't filter when there's less than 2 options
        if (locations.Count > 1)
        {
            // Specific location
            locations = locations.Where(location => location != "any").ToList();
            if (locations.Count == 0)
            {
                // Never should reach this if everything works out
                logger.Error(localisationService.GetText("quest-repeatable_elimination_generation_failed_please_report"));

                locationKey = null;
                return false;
            }
        }

        // Get name of location we want elimination to occur on
        locationKey = randomUtil.DrawRandomFromList(locations).First();

        // Get a pool of locations the chosen bot type can be eliminated on
        if (!targetPool.Targets!.TryGetValue(botTypeToEliminate, out var possibleLocationPool))
        {
            logger.Warning($"Bot to kill: {botTypeToEliminate} not found in elimination dict");

            locationKey = null;
            return false;
        }

        // Can't use out params in lambda's
        var tmpKey = locationKey;

        // Filter locations bot can be killed on to just those not chosen by key
        possibleLocationPool.Locations = possibleLocationPool.Locations?.Where(location => location != tmpKey).ToList();

        // None left after filtering
        if (possibleLocationPool.Locations?.Count is null or 0)
        {
            // TODO: Why do any of this?!
            // Remove chosen bot to eliminate from pool
            targetPool.Targets.Remove(botTypeToEliminate);
        }

        return true;
    }

    /// <summary>
    ///     Selects body parts to add to the condition. Modifies the bodyPartDifficulty based on selection.
    /// </summary>
    /// <param name="generationData">Generation data</param>
    /// <param name="bodyPartDifficulty">BodyPartDifficulty to modify based on selection</param>
    /// <returns>List of selected body parts</returns>
    protected List<string> GenerateBodyParts(EliminationQuestGenerationData generationData, ref double bodyPartDifficulty)
    {
        // if we add a bodyPart condition, we draw randomly one or two parts
        // each bodyPart of the BODYPARTS ProbabilityObjectArray includes the string(s)
        // which need to be presented to the client in ProbabilityObjectArray.data
        // e.g. we draw "Arms" from the probability array but must present ["LeftArm", "RightArm"] to the client
        var bodyPartsToClient = new List<string>();

        var bodyParts = generationData.BodyPartsConfig.Draw(randomUtil.RandInt(1, 3), false);

        var probability = 0d;

        foreach (var bodyPart in bodyParts)
        {
            // more than one part lead to an "OR" condition hence more parts reduce the difficulty
            probability += generationData.BodyPartsConfig?.Probability(bodyPart) ?? 0d;

            // Add multiple body parts needed for key
            if (_bodyPartsToClient.TryGetValue(bodyPart, out var bodyPartListToClient))
            {
                bodyPartsToClient.AddRange(bodyPartListToClient);
                continue;
            }

            // Add singular body-part, e.g. head
            bodyPartsToClient.Add(bodyPart);
        }

        bodyPartDifficulty = 1 / probability;

        return bodyPartsToClient;
    }

    /// <summary>
    ///     Determines if we're allowed to generate a distance requirement for this location.
    /// Takes into account location whitelist, random chance, and boss location modifiers
    /// </summary>
    /// <param name="generationData">Generation data</param>
    /// <param name="botTypeToEliminate">Bot type to eliminate</param>
    /// <param name="locationKey">Location key to check</param>
    /// <param name="targetsConfig">Targets config</param>
    /// <returns>True if allowed, false if not</returns>
    protected bool IsDistanceRequirementAllowed(
        EliminationQuestGenerationData generationData,
        string botTypeToEliminate,
        string locationKey,
        ProbabilityObjectArray<string, BossInfo> targetsConfig
    )
    {
        // This location is can be chosen for a distance requirement
        var whitelisted = !generationData.EliminationConfig.DistLocationBlacklist.Contains(locationKey);

        // We're not whitelisted, exit early to avoid doing a roll for no reason
        if (!whitelisted)
        {
            return false;
        }

        // Are we allowed a distance condition by chance?
        var isAllowedByChance = randomUtil.GetChance100(generationData.EliminationConfig.DistanceProbability);

        // Not allowed by chance, return early.
        // We now just assume we rolled this condition and don't take it into account anymore.
        if (!isAllowedByChance)
        {
            return false;
        }

        // We're not a boss, return true if this location is whitelisted
        if (!(targetsConfig.Data(botTypeToEliminate)?.IsBoss ?? false))
        {
            return whitelisted;
        }

        // Get all boss spawn information
        var bossSpawns = databaseService
            .GetLocations()
            .GetDictionary()
            .Select(x => x.Value)
            .Where(location => location.Base?.Id != null)
            .Select(location => new { location.Base.Id, BossSpawn = location.Base.BossLocationSpawn });

        // filter for the current boss to spawn on map
        var thisBossSpawns = bossSpawns
            .Select(x => new { x.Id, BossSpawn = x.BossSpawn.Where(e => e.BossName == botTypeToEliminate) })
            .Where(x => x.BossSpawn.Any());

        // remove blacklisted locations
        var allowedSpawns = thisBossSpawns.Where(x => !generationData.EliminationConfig.DistLocationBlacklist.Contains(x.Id));

        // if the boss spawns on non-blacklisted locations and the current location is allowed,
        // we can generate a distance kill requirement
        return whitelisted && allowedSpawns.Any();
    }

    /// <summary>
    ///     Generate a distance requirement and difficulty modifier
    /// </summary>
    /// <param name="generationData">Generation data</param>
    /// <returns>distance and difficulty modifier</returns>
    protected (int, int) GenerateDistanceRequirement(EliminationQuestGenerationData generationData)
    {
        // Random distance with lower values more likely; simple distribution for starters...
        var distance = (int)
            Math.Floor(
                Math.Abs(randomUtil.Random.NextDouble() - randomUtil.Random.NextDouble())
                    * (1 + generationData.EliminationConfig.MaxDistance - generationData.EliminationConfig.MinDistance)
                    + generationData.EliminationConfig.MinDistance
            );

        distance = (int)Math.Ceiling((decimal)(distance / 5d)) * 5;

        var distanceDifficulty = (int)(MaxDistDifficulty * distance / generationData.EliminationConfig.MaxDistance);

        return (distance, distanceDifficulty);
    }

    /// <summary>
    ///     Generate a weapon category requirement
    /// </summary>
    /// <param name="generationData">Generation data</param>
    /// <param name="distance">Distance to generate it for, pass null if not required</param>
    /// <returns>Weapon requirement category selected</returns>
    protected string? GenerateWeaponCategoryRequirement(EliminationQuestGenerationData generationData, int? distance)
    {
        switch (distance)
        {
            // Filter out close range weapons from far distance requirement
            case > 50:
            {
                HashSet<string> weaponTypeBlacklist = ["Shotgun", "Pistol"];

                // Filter out close range weapons from long distance requirement
                generationData.WeaponCategoryRequirementConfig.RemoveAll(category => weaponTypeBlacklist.Contains(category.Key));
                break;
            }
            // Filter out long range weapons from close distance requirement
            case < 20:
            {
                HashSet<string> weaponTypeBlacklist = ["MarksmanRifle", "DMR"];

                // Filter out far range weapons from close distance requirement
                generationData.WeaponCategoryRequirementConfig.RemoveAll(category => weaponTypeBlacklist.Contains(category.Key));
                break;
            }
        }

        // Pick a weighted weapon category
        var weaponRequirement = generationData.WeaponCategoryRequirementConfig.Draw(1, false);

        // Get the hideout id value stored in the .data array
        return generationData.WeaponCategoryRequirementConfig.Data(weaponRequirement[0])?[0];
    }

    /// <summary>
    ///     Generate a specific weapon to use, only use this if we aren't already using a weapon category requirement
    /// </summary>
    /// <param name="generationData">Generation data</param>
    /// <returns>Weapon to use</returns>
    protected MongoId GenerateSpecificWeaponRequirement(EliminationQuestGenerationData generationData)
    {
        var weaponRequirement = generationData.WeaponRequirementConfig.Draw(1, false);
        var specificAllowedWeaponCategory = generationData.WeaponRequirementConfig.Data(weaponRequirement[0]);

        if (specificAllowedWeaponCategory?[0] is null)
        {
            logger.Error(localisationService.GetText("repeatable-elimination-specific-weapon-null"));
            return MongoId.Empty();
        }

        var allowedWeapons = itemHelper.GetItemTplsOfBaseType(specificAllowedWeaponCategory[0]);

        return randomUtil.GetArrayValue(allowedWeapons);
    }

    /// <summary>
    ///     Get a number of kills needed to complete elimination quest
    /// </summary>
    /// <param name="targetKey"> Target type desired e.g. anyPmc/bossBully/Savage </param>
    /// <param name="targetsConfig"> Config of the target </param>
    /// <param name="eliminationConfig"> Config of the elimination </param>
    /// <returns> Number of AI to kill </returns>
    protected int GetEliminationKillCount(
        string targetKey,
        ProbabilityObjectArray<string, BossInfo> targetsConfig,
        EliminationConfig eliminationConfig
    )
    {
        if (targetsConfig.Data(targetKey)?.IsBoss ?? false)
        {
            return randomUtil.RandInt(eliminationConfig.MinBossKills, eliminationConfig.MaxBossKills + 1);
        }

        if (targetsConfig.Data(targetKey)?.IsPmc ?? false)
        {
            return randomUtil.RandInt(eliminationConfig.MinPmcKills, eliminationConfig.MaxPmcKills + 1);
        }

        return randomUtil.RandInt(eliminationConfig.MinKills, eliminationConfig.MaxKills + 1);
    }

    protected double DifficultyWeighing(double target, double bodyPart, int dist, int kill, int weaponRequirement)
    {
        return Math.Sqrt(Math.Sqrt(target) + bodyPart + dist + weaponRequirement) * kill;
    }

    /// <summary>
    ///     A repeatable quest, besides some more or less static components, exists of reward and condition (see
    ///     assets/database/templates/repeatableQuests.json)
    ///     This is a helper method for GenerateEliminationQuest to create a location condition.
    /// </summary>
    /// <param name="location">the location on which to fulfill the elimination quest</param>
    /// <returns>Elimination-location-subcondition object</returns>
    protected QuestConditionCounterCondition GenerateEliminationLocation(List<string> location)
    {
        return new QuestConditionCounterCondition
        {
            Id = new MongoId(),
            DynamicLocale = true,
            Target = new ListOrT<string>(location, null),
            ConditionType = "Location",
        };
    }

    /// <summary>
    ///     Create kill condition for an elimination quest
    /// </summary>
    /// <param name="target">Bot type target of elimination quest e.g. "AnyPmc", "Savage"</param>
    /// <param name="targetedBodyParts">Body parts player must hit</param>
    /// <param name="distance">Distance from which to kill (currently only >= supported)</param>
    /// <param name="allowedWeapon">What weapon must be used - undefined = any</param>
    /// <param name="allowedWeaponCategory">What category of weapon must be used - undefined = any</param>
    /// <returns>EliminationCondition object</returns>
    protected QuestConditionCounterCondition GenerateEliminationCondition(
        string target,
        List<string>? targetedBodyParts,
        double? distance,
        string? allowedWeapon,
        string? allowedWeaponCategory
    )
    {
        var killConditionProps = new QuestConditionCounterCondition
        {
            Id = new MongoId(),
            DynamicLocale = true,
            Target = new ListOrT<string>(null, target), // e,g, "AnyPmc"
            Value = 1,
            ResetOnSessionEnd = false,
            EnemyHealthEffects = [],
            Daytime = new DaytimeCounter { From = 0, To = 0 },
            ConditionType = "Kills",
        };

        if (target.StartsWith("boss"))
        {
            killConditionProps.Target = new ListOrT<string>(null, "Savage");
            killConditionProps.SavageRole = [target];
        }

        // Has specific body part hit condition
        if (targetedBodyParts is not null)
        {
            killConditionProps.BodyPart = targetedBodyParts;
        }

        // Don't allow distance + melee requirement
        if (distance is not null && allowedWeaponCategory != "5b5f7a0886f77409407a7f96")
        {
            killConditionProps.Distance = new CounterConditionDistance { CompareMethod = ">=", Value = distance.Value };
        }

        // Has specific weapon requirement
        if (allowedWeapon is not null)
        {
            killConditionProps.Weapon = [allowedWeapon];
        }

        // Has specific weapon category requirement
        if (allowedWeaponCategory?.Length > 0)
        {
            // TODO - fix - does weaponCategories exist?
            // killConditionProps.weaponCategories = [allowedWeaponCategory];
        }

        return killConditionProps;
    }
}
