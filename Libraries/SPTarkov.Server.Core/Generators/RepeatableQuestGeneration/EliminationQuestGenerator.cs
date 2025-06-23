using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
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
    HashUtil hashUtil,
    MathUtil mathUtil,
    RepeatableQuestHelper repeatableQuestHelper,
    ItemHelper itemHelper,
    RepeatableQuestRewardGenerator repeatableQuestRewardGenerator,
    DatabaseService databaseService,
    LocalisationService localisationService,
    ConfigServer configServer,
    ICloner cloner
) : IRepeatableQuestGenerator
{
    /// <summary>
    /// Body parts to present to the client as opposed to the body part information in quest data.
    /// </summary>
    private static readonly Dictionary<string, List<string>> _bodyPartsToClient = new()
    {
        { BodyParts.Arms, [BodyParts.LeftArm, BodyParts.RightArm] },
        { BodyParts.Legs, [BodyParts.LeftLeg, BodyParts.RightLeg] },
        { BodyParts.Head, [BodyParts.Head] },
        { BodyParts.Chest, [BodyParts.Chest, BodyParts.Stomach] },
    };

    protected QuestConfig QuestConfig = configServer.GetConfig<QuestConfig>();

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
        string sessionId,
        int pmcLevel,
        string traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
    )
    {
        var rand = new Random();

        var eliminationConfig = repeatableQuestHelper.GetEliminationConfigByPmcLevel(
            pmcLevel,
            repeatableConfig
        );
        var locationsConfig = repeatableConfig.Locations;
        var targetsConfig = new ProbabilityObjectArray<string, BossInfo>(
            mathUtil,
            cloner,
            eliminationConfig.Targets
        );
        var bodyPartsConfig = new ProbabilityObjectArray<string, List<string>>(
            mathUtil,
            cloner,
            eliminationConfig.BodyParts
        );
        var weaponCategoryRequirementConfig = new ProbabilityObjectArray<string, List<string>>(
            mathUtil,
            cloner,
            eliminationConfig.WeaponCategoryRequirements
        );
        var weaponRequirementConfig = new ProbabilityObjectArray<string, List<string>>(
            mathUtil,
            cloner,
            eliminationConfig.WeaponRequirements
        );

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

        // We want to generate a reward which is scaled by the difficulty of this mission. To get a upper bound with which we scale
        // the actual difficulty we calculate the minimum and maximum difficulty (max being the sum of max of each condition type
        // times the number of kills we have to perform):

        // The minimum difficulty is the difficulty for the most probable (= easiest target) with no additional conditions
        var minDifficulty = 1 / targetsConfig.MaxProbability(); // min difficulty is the lowest amount of scavs without any constraints

        // Target on bodyPart max. difficulty is that of the least probable element
        var maxTargetDifficulty = 1 / targetsConfig.MinProbability();
        var maxBodyPartsDifficulty = eliminationConfig.MinKills / bodyPartsConfig.MinProbability();

        // maxDistDifficulty is defined by 2, this could be a tuning parameter if we don't like the reward generation
        const int maxDistDifficulty = 2;

        var maxKillDifficulty = eliminationConfig.MaxKills;

        var targetPool = questTypePool.Pool.Elimination;
        targetsConfig = targetsConfig.Filter(x => targetPool.Targets.ContainsKey(x.Key));

        if (targetsConfig.Count == 0 || targetsConfig.All(x => x.Data?.IsBoss ?? false))
        {
            // There are no more targets left for elimination; delete it as a possible quest type
            // also if only bosses are left we need to leave otherwise it's a guaranteed boss elimination
            // -> then it would not be a quest with low probability anymore
            questTypePool.Types = questTypePool.Types.Where(t => t != "Elimination").ToList();
            return null;
        }

        var botTypeToEliminate = targetsConfig.Draw()[0];
        var targetDifficulty = 1 / targetsConfig.Probability(botTypeToEliminate);

        targetPool.Targets.TryGetValue(botTypeToEliminate, out var targetLocationPool);
        var locations = targetLocationPool.Locations;

        // we use any as location if "any" is in the pool, and we don't hit the specific location random
        // we use any also if the random condition is not met in case only "any" was in the pool
        var locationKey = "any";
        if (
            locations.Contains("any")
            && (
                randomUtil.GetChance100(eliminationConfig.SpecificLocationChance)
                || locations.Count <= 1
            )
        )
        {
            locationKey = "any";
            targetPool.Targets.Remove(botTypeToEliminate);
        }
        else
        {
            // Specific location
            locations = locations.Where(l => l != "any").ToList();
            if (locations.Count > 0)
            {
                // Get name of location we want elimination to occur on
                locationKey = randomUtil.DrawRandomFromList(locations).FirstOrDefault();

                // Get a pool of locations the chosen bot type can be eliminated on
                if (
                    !targetPool.Targets.TryGetValue(
                        botTypeToEliminate,
                        out var possibleLocationPool
                    )
                )
                {
                    logger.Warning(
                        $"Bot to kill: {botTypeToEliminate} not found in elimination dict"
                    );
                }

                // Filter locations bot can be killed on to just those not chosen by key
                possibleLocationPool.Locations = possibleLocationPool
                    .Locations.Where(location => location != locationKey)
                    .ToList();

                // None left after filtering
                if (possibleLocationPool.Locations.Count == 0)
                {
                    // TODO: Why do any of this?!
                    // Remove chosen bot to eliminate from pool
                    targetPool.Targets.Remove(botTypeToEliminate);
                }
            }
            else
            {
                // Never should reach this if everything works out
                logger.Error(
                    localisationService.GetText(
                        "quest-repeatable_elimination_generation_failed_please_report"
                    )
                );
            }
        }

        // draw the target body part and calculate the difficulty factor
        var bodyPartsToClient = new List<string>();
        var bodyPartDifficulty = 0d;
        if (randomUtil.GetChance100(eliminationConfig.BodyPartChance))
        {
            // if we add a bodyPart condition, we draw randomly one or two parts
            // each bodyPart of the BODYPARTS ProbabilityObjectArray includes the string(s) which need to be presented to the client in ProbabilityObjectArray.data
            // e.g. we draw "Arms" from the probability array but must present ["LeftArm", "RightArm"] to the client
            bodyPartsToClient = [];
            var bodyParts = bodyPartsConfig.Draw(randomUtil.RandInt(1, 3), false);
            double probability = 0;
            foreach (var bodyPart in bodyParts)
            {
                // more than one part lead to an "OR" condition hence more parts reduce the difficulty
                probability += bodyPartsConfig.Probability(bodyPart).Value;

                if (_bodyPartsToClient.TryGetValue(bodyPart, out var bodyPartListToClient))
                {
                    bodyPartsToClient.AddRange(bodyPartListToClient);
                }
                else
                {
                    bodyPartsToClient.Add(bodyPart);
                }
            }

            bodyPartDifficulty = 1 / probability;
        }

        // Draw a distance condition
        int? distance = null;
        var distanceDifficulty = 0;
        var isDistanceRequirementAllowed = !eliminationConfig.DistLocationBlacklist.Contains(
            locationKey
        );

        if (targetsConfig.Data(botTypeToEliminate)?.IsBoss ?? false)
        {
            // Get all boss spawn information
            var bossSpawns = databaseService
                .GetLocations()
                .GetDictionary()
                .Select(x => x.Value)
                .Where(x => x.Base?.Id != null)
                .Select(x => new { x.Base.Id, BossSpawn = x.Base.BossLocationSpawn });
            // filter for the current boss to spawn on map
            var thisBossSpawns = bossSpawns
                .Select(x => new
                {
                    x.Id,
                    BossSpawn = x.BossSpawn.Where(e => e.BossName == botTypeToEliminate),
                })
                .Where(x => x.BossSpawn.Count() > 0);
            // remove blacklisted locations
            var allowedSpawns = thisBossSpawns.Where(x =>
                !eliminationConfig.DistLocationBlacklist.Contains(x.Id)
            );
            // if the boss spawns on nom-blacklisted locations and the current location is allowed we can generate a distance kill requirement
            isDistanceRequirementAllowed = isDistanceRequirementAllowed && allowedSpawns.Any();
        }

        if (
            randomUtil.GetChance100(eliminationConfig.DistanceProbability)
            && isDistanceRequirementAllowed
        )
        {
            // Random distance with lower values more likely; simple distribution for starters...
            distance = (int)
                Math.Floor(
                    Math.Abs(rand.NextDouble() - rand.NextDouble())
                        * (1 + eliminationConfig.MaxDistance - eliminationConfig.MinDistance)
                        + eliminationConfig.MinDistance
                );

            distance = (int)Math.Ceiling((decimal)(distance / 5)) * 5;
            distanceDifficulty = (int)(
                maxDistDifficulty * distance / eliminationConfig.MaxDistance
            );
        }

        string? allowedWeaponsCategory = null;
        if (randomUtil.GetChance100(eliminationConfig.WeaponCategoryRequirementProbability))
        {
            // Filter out close range weapons from far distance requirement
            if (distance > 50)
            {
                List<string> weaponTypeBlacklist = ["Shotgun", "Pistol"];

                // Filter out close range weapons from long distance requirement
                weaponCategoryRequirementConfig.RemoveAll(category =>
                    weaponTypeBlacklist.Contains(category.Key)
                );
            }
            else if (distance < 20)
            {
                List<string> weaponTypeBlacklist = ["MarksmanRifle", "DMR"];

                // Filter out far range weapons from close distance requirement
                weaponCategoryRequirementConfig.RemoveAll(category =>
                    weaponTypeBlacklist.Contains(category.Key)
                );
            }

            // Pick a weighted weapon category
            var weaponRequirement = weaponCategoryRequirementConfig.Draw(1, false);

            // Get the hideout id value stored in the .data array
            allowedWeaponsCategory = weaponCategoryRequirementConfig.Data(weaponRequirement[0])[0];
        }

        // Only allow a specific weapon requirement if a weapon category was not chosen
        string? allowedWeapon = null;
        if (
            allowedWeaponsCategory is not null
            && eliminationConfig.WeaponRequirementProbability > rand.NextDouble()
        )
        {
            var weaponRequirement = weaponRequirementConfig.Draw(1, false);
            var specificAllowedWeaponCategory = weaponRequirementConfig.Data(weaponRequirement[0]);
            var allowedWeapons = itemHelper.GetItemTplsOfBaseType(
                specificAllowedWeaponCategory[0]
            );
            allowedWeapon = randomUtil.GetArrayValue(allowedWeapons);
        }

        // Draw how many npm kills are required
        var desiredKillCount = GetEliminationKillCount(
            botTypeToEliminate,
            targetsConfig,
            eliminationConfig
        );
        var killDifficulty = desiredKillCount;

        // not perfectly happy here; we give difficulty = 1 to the quest reward generation when we have the most difficult mission
        // e.g. killing reshala 5 times from a distance of 200m with a headshot.
        var maxDifficulty = DifficultyWeighing(1, 1, 1, 1, 1);
        var curDifficulty = DifficultyWeighing(
            targetDifficulty.Value / maxTargetDifficulty,
            bodyPartDifficulty / maxBodyPartsDifficulty,
            distanceDifficulty / maxDistDifficulty,
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

        // ASSUMPTION: All fence quests are for scavs
        if (traderId == Traders.FENCE)
        {
            quest.Side = "Scav";
        }

        var availableForFinishCondition = quest.Conditions.AvailableForFinish[0];
        availableForFinishCondition.Counter.Id = hashUtil.Generate();
        availableForFinishCondition.Counter.Conditions = [];

        // Only add specific location condition if specific map selected
        if (locationKey != "any")
        {
            var locationId = Enum.Parse<ELocationName>(locationKey);
            availableForFinishCondition.Counter.Conditions.Add(
                GenerateEliminationLocation(locationsConfig[locationId])
            );
        }

        availableForFinishCondition.Counter.Conditions.Add(
            GenerateEliminationCondition(
                botTypeToEliminate,
                bodyPartsToClient,
                distance,
                allowedWeapon,
                allowedWeaponsCategory
            )
        );
        availableForFinishCondition.Value = desiredKillCount;
        availableForFinishCondition.Id = hashUtil.Generate();

        // Get the quest location, default to any if none exist
        quest.Location = repeatableQuestHelper.GetQuestLocationByMapId(locationKey) ?? "any";

        quest.Rewards = repeatableQuestRewardGenerator.GenerateReward(
            pmcLevel,
            Math.Min(difficulty, 1),
            traderId,
            repeatableConfig,
            eliminationConfig
        );

        return quest;
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
            return randomUtil.RandInt(
                eliminationConfig.MinBossKills,
                eliminationConfig.MaxBossKills + 1
            );
        }

        if (targetsConfig.Data(targetKey)?.IsPmc ?? false)
        {
            return randomUtil.RandInt(
                eliminationConfig.MinPmcKills,
                eliminationConfig.MaxPmcKills + 1
            );
        }

        return randomUtil.RandInt(eliminationConfig.MinKills, eliminationConfig.MaxKills + 1);
    }

    protected double DifficultyWeighing(
        double target,
        double bodyPart,
        int dist,
        int kill,
        int weaponRequirement
    )
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
            Id = hashUtil.Generate(),
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
            Id = hashUtil.Generate(),
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
            killConditionProps.Distance = new CounterConditionDistance
            {
                CompareMethod = ">=",
                Value = distance.Value,
            };
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
