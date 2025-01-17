using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Health;
using Core.Models.Eft.Hideout;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Repeatable;
using Core.Models.Utils;
using Core.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using Core.Helpers;
using Core.Services;
using Core.Utils.Collections;
using BodyPart = Core.Models.Spt.Config.BodyPart;

namespace Core.Generators;

[Injectable]
public class RepeatableQuestGenerator
{
    protected ISptLogger<RepeatableQuestGenerator> _logger;
    protected RandomUtil _randomUtil;
    private readonly HashUtil _hashUtil;
    private readonly MathUtil _mathUtil;
    private readonly RepeatableQuestHelper _repeatableQuestHelper;
    private readonly ItemHelper _itemHelper;
    private readonly RepeatableQuestRewardGenerator _repeatableQuestRewardGenerator;
    private readonly DatabaseService _databaseService;

    public RepeatableQuestGenerator(
        ISptLogger<RepeatableQuestGenerator> logger,
        RandomUtil randomUtil,
        HashUtil hashUtil,
        MathUtil mathUtil,
        RepeatableQuestHelper repeatableQuestHelper,
        ItemHelper itemHelper,
        RepeatableQuestRewardGenerator repeatableQuestRewardGenerator,
        DatabaseService databaseService
        )
    {
        _logger = logger;
        _randomUtil = randomUtil;
        _hashUtil = hashUtil;
        _mathUtil = mathUtil;
        _repeatableQuestHelper = repeatableQuestHelper;
        _itemHelper = itemHelper;
        _repeatableQuestRewardGenerator = repeatableQuestRewardGenerator;
        _databaseService = databaseService;
    }

    /// <summary>
    /// This method is called by /GetClientRepeatableQuests/ and creates one element of quest type format (see assets/database/templates/repeatableQuests.json).
    /// It randomly draws a quest type (currently Elimination, Completion or Exploration) as well as a trader who is providing the quest
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcLevel">Player's level for requested items and reward generation</param>
    /// <param name="pmcTraderInfo">Players traper standing/rep levels</param>
    /// <param name="questTypePool">Possible quest types pool</param>
    /// <param name="repeatableConfig">Repeatable quest config</param>
    /// <returns>RepeatableQuest</returns>
    public RepeatableQuest? GenerateRepeatableQuest(
        string sessionId,
        int pmcLevel,
        Dictionary<string, TraderInfo> pmcTraderInfo,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
    )
    {
        var questType = _randomUtil.DrawRandomFromList(questTypePool.Types).First();

        // Get traders from whitelist and filter by quest type availability
        var traders = repeatableConfig.TraderWhitelist
            .Where((x) => x.QuestTypes.Contains(questType))
            .Select((x) => x.TraderId).ToList();
        // filter out locked traders
        traders = traders.Where((x) => pmcTraderInfo[x].Unlocked.GetValueOrDefault(false)).ToList();
        var traderId = _randomUtil.DrawRandomFromList(traders).FirstOrDefault();

        return questType switch
        {
            "Elimination" => GenerateEliminationQuest(sessionId, pmcLevel, traderId, questTypePool, repeatableConfig),
            "Completion" => GenerateCompletionQuest(sessionId, pmcLevel, traderId, repeatableConfig),
            "Exploration" => GenerateExplorationQuest(sessionId, pmcLevel, traderId, questTypePool, repeatableConfig),
            "Pickup" => GeneratePickupQuest(sessionId, pmcLevel, traderId, questTypePool, repeatableConfig),
            _ => null
        };
    }

    /// <summary>
    /// Generate a randomised Elimination quest
    /// </summary>
    /// <param name="pmcLevel">Player's level for requested items and reward generation</param>
    /// <param name="traderId">Trader from which the quest will be provided</param>
    /// <param name="questTypePool">Pools for quests (used to avoid redundant quests)</param>
    /// <param name="repeatableConfig">The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig for the requestd quest</param>
    /// <returns>Object of quest type format for "Elimination" (see assets/database/templates/repeatableQuests.json)</returns>
    protected RepeatableQuest GenerateEliminationQuest(
        string sessionid,
        int pmcLevel,
        string traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
    )
    {
        var eliminationConfig = _repeatableQuestHelper.GetEliminationConfigByPmcLevel(pmcLevel, repeatableConfig);
        var locationsConfig = repeatableConfig.Locations;
        var targetsConfig = _repeatableQuestHelper.ProbabilityObjectArray<Target, string, BossInfo>(eliminationConfig.Targets);
        var bodypartsConfig = _repeatableQuestHelper.ProbabilityObjectArray<BodyPart, string, List<string>>(eliminationConfig.BodyParts);
        var weaponCategoryRequirementConfig = _repeatableQuestHelper.ProbabilityObjectArray<WeaponRequirement, string, List<string>>(eliminationConfig.WeaponCategoryRequirements);
        var weaponRequirementConfig = _repeatableQuestHelper.ProbabilityObjectArray<WeaponRequirement, string, List<string>>(eliminationConfig.WeaponRequirements);

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

        // the minimum difficulty is the difficulty for the most probable (= easiest target) with no additional conditions
        var minDifficulty = 1 / targetsConfig.MaxProbability(); // min difficulty is lowest amount of scavs without any constraints

        // Target on bodyPart max. difficulty is that of the least probable element
        var maxTargetDifficulty = 1 / targetsConfig.MinProbability();
        var maxBodyPartsDifficulty = eliminationConfig.MinKills / bodypartsConfig.MinProbability();

        // maxDistDifficulty is defined by 2, this could be a tuning parameter if we don't like the reward generation
        var maxDistDifficulty = 2;

        var maxKillDifficulty = eliminationConfig.MaxKills;

        targetsConfig = targetsConfig.Where((x) =>
            (questTypePool.Pool.Elimination.Targets).Contains(x.key));
        if (targetsConfig.Count == 0 || targetsConfig.All((x) => x.Data.isBoss))
        {
            // There are no more targets left for elimination; delete it as a possible quest type
            // also if only bosses are left we need to leave otherwise it's a guaranteed boss elimination
            // -> then it would not be a quest with low probability anymore
            questTypePool.Types = questTypePool.Types.Where((t) => t != "Elimination");
            return null;
        }

        var targetKey = targetsConfig.Draw()[0];
        var targetDifficulty = 1 / targetsConfig.Probability(targetKey);

        var locations: string[] = questTypePool.Pool.Elimination.Targets[targetKey].locations;

        // we use any as location if "any" is in the pool and we do not hit the specific location random
        // we use any also if the random condition is not met in case only "any" was in the pool
        var locationKey = "any";
        if (locations.includes("any") &&
            (eliminationConfig.specificLocationProb < Math.random() || locations.length <= 1)
        )
        {
            locationKey = "any";
            questTypePool.Pool.Elimination.Targets.Remove(targetKey);
        }
        else
        {
            locations = locations.Where((l) => l != "any");
            if (locations.length > 0)
            {
                locationKey = _randomUtil.DrawRandomFromList(locations)[0];
                questTypePool.Pool.Elimination.Targets[targetKey].locations = locations.Where(
                    (l) => l != locationKey);
                if (questTypePool.Pool.Elimination.Targets[targetKey].locations.length == 0)
                {
                    questTypePool.Pool.Elimination.Targets.Remove(targetKey);
                }
            }
            else
            {
                // never should reach this if everything works out
                _logger.Debug("Encountered issue when creating Elimination quest. Please report.");
            }
        }

        // draw the target body part and calculate the difficulty factor
        var bodyPartsToClient = null;
        var bodyPartDifficulty = 0;
        if (eliminationConfig.BodyPartProb > Math.Random())
        {
            // if we add a bodyPart condition, we draw randomly one or two parts
            // each bodyPart of the BODYPARTS ProbabilityObjectArray includes the string(s) which need to be presented to the client in ProbabilityObjectArray.data
            // e.g. we draw "Arms" from the probability array but must present ["LeftArm", "RightArm"] to the client
            bodyPartsToClient = [];
            var bodyParts = bodypartsConfig.Draw(_randomUtil.RandInt(1, 3), false);
            var probability = 0;
            foreach (var bi in bodyParts) {
                // more than one part lead to an "OR" condition hence more parts reduce the difficulty
                probability += bodypartsConfig.Probability(bi);
                foreach (var biClient in bodypartsConfig.Data(bi)) {
                    bodyPartsToClient.push(biClient);
                }
            }
            bodyPartDifficulty = 1 / probability;
        }

        // Draw a distance condition
        var distance = null;
        var distanceDifficulty = 0;
        var isDistanceRequirementAllowed = !eliminationConfig.DistLocationBlacklist.Contains(locationKey);

        if (targetsConfig.Data(targetKey).isBoss)
        {
            // Get all boss spawn information
            var bossSpawns = _databaseService.GetLocations().GetDictionary()
                .Where((x) => "base" in x && "Id" in x.base)
                .Select((x) => ({ Id: x.base.Id, BossSpawn: x.base.BossLocationSpawn }));
            // filter for the current boss to spawn on map
            var thisBossSpawns = bossSpawns.Select((x) => ({ Id: x.Id,BossSpawn: x.BossSpawn.Where((e) => e.BossName == targetKey)})).Where((x) => x.BossSpawn.length > 0);
            // remove blacklisted locations
            var allowedSpawns = thisBossSpawns.filter((x) => !eliminationConfig.distLocationBlacklist.includes(x.Id));
            // if the boss spawns on nom-blacklisted locations and the current location is allowed we can generate a distance kill requirement
            isDistanceRequirementAllowed = isDistanceRequirementAllowed && allowedSpawns.length > 0;
        }

        if (eliminationConfig.DistProb > Math.Random() && isDistanceRequirementAllowed)
        {
            // Random distance with lower values more likely; simple distribution for starters...
            distance = Math.Floor(
                Math.Abs(Math.random() - Math.Random()) * (1 + eliminationConfig.MaxDist - eliminationConfig.MinDist) +
                    eliminationConfig.MinDist);
            distance = Math.Ceiling(distance / 5) * 5;
            distanceDifficulty = (maxDistDifficulty * distance) / eliminationConfig.MaxDist;
        }

        string allowedWeaponsCategory;
        if (eliminationConfig.WeaponCategoryRequirementProb > Math.Random())
        {
            // Filter out close range weapons from far distance requirement
            if (distance > 50)
            {
                weaponCategoryRequirementConfig = weaponCategoryRequirementConfig.Where((category) =>
                    ["Shotgun", "Pistol"].Contains(category.key));
            }
            else if (distance < 20)
            {
                // Filter out far range weapons from close distance requirement
                weaponCategoryRequirementConfig = weaponCategoryRequirementConfig.Where((category) =>
                    ["MarksmanRifle", "DMR"].Contains(category.key));
            }

            // Pick a weighted weapon category
            var weaponRequirement = weaponCategoryRequirementConfig.Draw(1, false);

            // Get the hideout id value stored in the .data array
            allowedWeaponsCategory = weaponCategoryRequirementConfig.Data(weaponRequirement[0])[0];
        }

        // Only allow a specific weapon requirement if a weapon category was not chosen
        string allowedWeapon;
        if (!allowedWeaponsCategory && eliminationConfig.WeaponRequirementProb > Math.Random())
        {
            var weaponRequirement = weaponRequirementConfig.Draw(1, false);
            var allowedWeaponsCategory = weaponRequirementConfig.Data(weaponRequirement[0])[0];
            var allowedWeapons = _itemHelper.GetItemTplsOfBaseType(allowedWeaponsCategory);
            allowedWeapon = _randomUtil.GetArrayValue(allowedWeapons);
        }

        // Draw how many npm kills are required
        var desiredKillCount = GetEliminationKillCount(targetKey, targetsConfig, eliminationConfig);
        var killDifficulty = desiredKillCount;

        // not perfectly happy here; we give difficulty = 1 to the quest reward generation when we have the most difficult mission
        // e.g. killing reshala 5 times from a distance of 200m with a headshot.
        var maxDifficulty = DifficultyWeighing(1, 1, 1, 1, 1);
        var curDifficulty = DifficultyWeighing(
            targetDifficulty / maxTargetDifficulty,
            bodyPartDifficulty / maxBodyPartsDifficulty,
            distanceDifficulty / maxDistDifficulty,
            killDifficulty / maxKillDifficulty,
            allowedWeaponsCategory || allowedWeapon ? 1 : 0);

        // Aforementioned issue makes it a bit crazy since now all easier quests give significantly lower rewards than Completion / Exploration
        // I therefore moved the mapping a bit up (from 0.2...1 to 0.5...2) so that normal difficulty still gives good reward and having the
        // crazy maximum difficulty will lead to a higher difficulty reward gain factor than 1
        var difficulty = _mathUtil.MapToRange(curDifficulty, minDifficulty, maxDifficulty, 0.5, 2);

        var quest = GenerateRepeatableTemplate("Elimination", traderId, repeatableConfig.Side, sessionid);

        // ASSUMPTION: All fence quests are for scavs
        if (traderId == Traders.FENCE)
        {
            quest.Side = "Scav";
        }

        var availableForFinishCondition = quest.Conditions.AvailableForFinish[0];
        availableForFinishCondition.Counter.Id = _hashUtil.Generate();
        availableForFinishCondition.Counter.Conditions = [];

        // Only add specific location condition if specific map selected
        if (locationKey != "any")
        {
            availableForFinishCondition.Counter.Conditions.Add(GenerateEliminationLocation(locationsConfig[locationKey]));
        }
        availableForFinishCondition.Counter.Conditions.Add(
            GenerateEliminationCondition(
                targetKey,
                bodyPartsToClient,
                distance,
                allowedWeapon,
                allowedWeaponsCategory)
        );
        availableForFinishCondition.Value = desiredKillCount;
        availableForFinishCondition.Id = _hashUtil.Generate();
        quest.Location = GetQuestLocationByMapId(locationKey);

        quest.Rewards = _repeatableQuestRewardGenerator.GenerateReward(
            pmcLevel,
            Math.Min(difficulty, 1),
            traderId,
            repeatableConfig,
            eliminationConfig);

        return quest;
    }

    /**
     * Get a number of kills needed to complete elimination quest
     * @param targetKey Target type desired e.g. anyPmc/bossBully/Savage
     * @param targetsConfig Config
     * @param eliminationConfig Config
     * @returns Number of AI to kill
     */
    protected int GetEliminationKillCount(
        string targetKey,
        object targetsConfig , //ProbabilityObjectArray<string, IBossInfo>
        EliminationConfig eliminationConfig) {
        if (targetsConfig.Data(targetKey).isBoss) {
            return _randomUtil.RandInt(eliminationConfig.MinBossKills, eliminationConfig.MaxBossKills + 1);
        }

        if (targetsConfig.Data(targetKey).isPmc) {
            return _randomUtil.RandInt(eliminationConfig.MinPmcKills, eliminationConfig.MaxPmcKills + 1);
        }

        return _randomUtil.RandInt(eliminationConfig.MinKills, eliminationConfig.MaxKills + 1);
    }

protected double DifficultyWeighing(
        int target,
        int bodyPart,
        int dist,
        int kill,
        int weaponRequirement)
    {
        return Math.Sqrt(Math.Sqrt(target) + bodyPart + dist + weaponRequirement) * kill;
    }
}

    /// <summary>
    /// Get a number of kills needed to complete elimination quest
    /// </summary>
    /// <param name="targetKey">Target type desired e.g. anyPmc/bossBully/Savage</param>
    /// <param name="targetsConfig">Config</param>
    /// <param name="eliminationConfig">Config</param>
    /// <returns>Number of AI to kill</returns>
    protected int GetEliminationKillCount(
        string targetKey,
        object targetsConfig, // TODO: typing was ProbabilityObjectArray<string, BossInfo>
        EliminationConfig eliminationConfig
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// A repeatable quest, besides some more or less static components, exists of reward and condition (see assets/database/templates/repeatableQuests.json)
    /// This is a helper method for GenerateEliminationQuest to create a location condition.
    /// </summary>
    /// <param name="location">the location on which to fulfill the elimination quest</param>
    /// <returns>Elimination-location-subcondition object</returns>
    protected QuestConditionCounterCondition GenerateEliminationLocation(List<string> location)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create kill condition for an elimination quest
    /// </summary>
    /// <param name="target">Bot type target of elimination quest e.g. "AnyPmc", "Savage"</param>
    /// <param name="targetedBodyParts">Body parts player must hit</param>
    /// <param name="distance">Distance from which to kill (currently only >= supported)</param>
    /// <param name="allowedWeapon">What weapon must be used - undefined = any</param>
    /// <param name="allowedWeaponCategory">What category of weapon must be used - undefined = any</param>
    /// <returns>EliminationCondition object</returns>
    protected QuestConditionCounterCondition GenerateEliminationCondition(
        string target,
        List<string> targetedBodyParts,
        double distance,
        string allowedWeapon,
        string allowedWeaponCategory
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generates a valid Completion quest
    /// </summary>
    /// <param name="pmcLevel">player's level for requested items and reward generation</param>
    /// <param name="traderId">trader from which the quest will be provided</param>
    /// <param name="repeatableConfig">The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig for the requested quest</param>
    /// <returns>quest type format for "Completion" (see assets/database/templates/repeatableQuests.json)</returns>
    protected RepeatableQuest GenerateCompletionQuest(
        string sessionId,
        int pmcLevel,
        string traderId,
        RepeatableQuestConfig repeatableConfig
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// A repeatable quest, besides some more or less static components, exists of reward and condition (see assets/database/templates/repeatableQuests.json)
    /// This is a helper method for GenerateCompletionQuest to create a completion condition (of which a completion quest theoretically can have many)
    /// </summary>
    /// <param name="itemTpl">id of the item to request</param>
    /// <param name="value">amount of items of this specific type to request</param>
    /// <returns>object of "Completion"-condition</returns>
    protected RepeatableQuest GenerateCompletionAvailableForFinish(string itemTpl, int value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generates a valid Exploration quest
    /// </summary>
    /// <param name="sessionId">session id for the quest</param>
    /// <param name="pmcLevel">player's level for reward generation</param>
    /// <param name="traderId">trader from which the quest will be provided</param>
    /// <param name="questTypePool">Pools for quests (used to avoid redundant quests)</param>
    /// <param name="repeatableConfig">The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig for the requested quest</param>
    /// <returns>object of quest type format for "Exploration" (see assets/database/templates/repeatableQuests.json)</returns>
    protected RepeatableQuest GenerateExplorationQuest(
        string sessionId,
        int pmcLevel,
        string traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Filter a maps exits to just those for the desired side
    /// </summary>
    /// <param name="locationKey">Map id (e.g. factory4_day)</param>
    /// <param name="playerSide">Scav/Pmc</param>
    /// <returns>List of Exit objects</returns>
    protected List<Exit> GetLocationExitsForSide(string locationKey, string playerSide)
    {
        throw new NotImplementedException();
    }

    protected RepeatableQuest GeneratePickupQuest(
        string sessionId,
        int pmcLevel,
        string traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Convert a location into an quest code can read (e.g. factory4_day into 55f2d3fd4bdc2d5f408b4567)
    /// </summary>
    /// <param name="locationKey">e.g factory4_day</param>
    /// <returns>guid</returns>
    protected string GetQuestLocationByMapId(string locationKey)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Exploration repeatable quests can specify a required extraction point.
    /// This method creates the according object which will be appended to the conditions list
    /// </summary>
    /// <param name="exit">The exit name to generate the condition for</param>
    /// <returns>Exit condition</returns>
    protected QuestConditionCounterCondition GenerateExplorationExitCondition(Exit exit)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generates the base object of quest type format given as templates in assets/database/templates/repeatableQuests.json
    /// The templates include Elimination, Completion and Extraction quest types
    /// </summary>
    /// <param name="type">Quest type: "Elimination", "Completion" or "Extraction"</param>
    /// <param name="traderId">Trader from which the quest will be provided</param>
    /// <param name="side">Scav daily or pmc daily/weekly quest</param>
    /// <returns>Object which contains the base elements for repeatable quests of the requests type
    /// (needs to be filled with reward and conditions by called to make a valid quest)</returns>
    protected RepeatableQuest GenerateRepeatableTemplate(
        string type,
        string traderId,
        string side,
        string sessionId)
    {
        throw new NotImplementedException();
    }
}
