using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Repeatable;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json;

namespace SPTarkov.Server.Core.Generators.RepeatableQuestGeneration;

[Injectable]
public class ExplorationQuestGenerator(
    ISptLogger<ExplorationQuestGenerator> logger,
    RepeatableQuestHelper repeatableQuestHelper,
    RepeatableQuestRewardGenerator repeatableQuestRewardGenerator,
    DatabaseService databaseService,
    LocalisationService localisationService,
    ConfigServer configServer,
    RandomUtil randomUtil,
    MathUtil mathUtil,
    HashUtil hashUtil
    ) : IRepeatableQuestGenerator
{
    protected record LocationInfo(
        ELocationName LocationName,
        List<string> LocationTarget,
        bool RequiresSpecificExtract,
        int NumOfExtractsRequired
        );

    protected QuestConfig QuestConfig = configServer.GetConfig<QuestConfig>();

    /// <summary>
    ///     Generates a valid Exploration quest
    /// </summary>
    /// <param name="sessionId">session id for the quest</param>
    /// <param name="pmcLevel">player's level for reward generation</param>
    /// <param name="traderId">trader from which the quest will be provided</param>
    /// <param name="questTypePool">Pools for quests (used to avoid redundant quests)</param>
    /// <param name="repeatableConfig">
    ///     The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig
    ///     for the requested quest
    /// </param>
    /// <returns>object of quest type format for "Exploration" (see assets/database/templates/repeatableQuests.json)</returns>
    public RepeatableQuest? Generate(
        string sessionId,
        int pmcLevel,
        string traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
    )
    {
        var explorationConfig = repeatableConfig.QuestConfig.Exploration;

        // Try and get a location to generate for
        if (!TryGetLocationInfo(repeatableConfig, explorationConfig, questTypePool, out var locationInfo)
            || locationInfo is null)
        {
            // TODO - Localize me
            logger.Warning("Generating exploration repeatable quest failed, no remaining locations available");
            return null;
        }

        // Generate the quest template
        var quest = repeatableQuestHelper.GenerateRepeatableTemplate(
            RepeatableQuestType.Exploration,
            traderId,
            repeatableConfig.Side,
            sessionId
        );

        if (quest is null)
        {
            // TODO - Localize me
            logger.Error("Generating quest failed, no quest template available");
            return null;
        }

        // Generate the available for finish exit condition
        if (!TryGenerateAvailableForFinish(quest, locationInfo))
        {
            // TODO - Localize me
            logger.Error($"Generating AvailableForFinish failed for location {locationInfo.LocationName}");
            return null;
        }

        // If we require a specific extract requirement, generate it
        if (locationInfo.RequiresSpecificExtract
            && !TryGenerateSpecificExtractRequirement(quest, repeatableConfig, locationInfo))
        {
            // TODO - Localize me
            logger.Error($"Generating SpecificExtractRequirement failed for location {locationInfo.LocationName}");
            return null;
        }

        // Difficulty for exploration goes from 1 extract to maxExtracts
        // Difficulty for reward goes from 0.2...1 -> map
        var difficulty = mathUtil.MapToRange(
            locationInfo.NumOfExtractsRequired,
            1,
            explorationConfig.MaximumExtracts,
            0.2,
            1
        );
        quest.Rewards = repeatableQuestRewardGenerator.GenerateReward(
            pmcLevel,
            difficulty,
            traderId,
            repeatableConfig,
            explorationConfig
        );

        return quest;
    }

    /// <summary>
    ///     Draws a location from the exploration location pool
    /// </summary>
    /// <param name="repeatableConfig"></param>
    /// <param name="explorationConfig"></param>
    /// <param name="pool">Pool to draw from</param>
    /// <param name="locationInfo">Location chosen</param>
    /// <returns>True if location selected, false if no locations remain</returns>
    protected bool TryGetLocationInfo(
        RepeatableQuestConfig repeatableConfig,
        Exploration explorationConfig,
        QuestTypePool pool,
        out LocationInfo? locationInfo)
    {
        if (pool.Pool?.Exploration?.Locations?.Count is null or 0)
        {
            // there are no more locations left for exploration; delete it as a possible quest type
            pool.Types = pool.Types?.Where(t => t != "Exploration").ToList();
            locationInfo = null;
            return false;
        }

        // If location drawn is factory, it's possible to either get factory4_day and factory4_night use index 0,
        // as the key is factory4_day
        var locationKey = randomUtil.DrawRandomFromDict(pool.Pool.Exploration.Locations)[
            0
        ];

        // Make the location info object
        var locationTarget = pool.Pool!.Exploration!.Locations![locationKey];

        var requiresSpecificExtract =
            randomUtil.GetChance100(repeatableConfig.QuestConfig.Exploration.SpecificExits.Chance);

        var numExtracts = GetNumberOfExits(explorationConfig, requiresSpecificExtract);

        locationInfo = new LocationInfo(locationKey,  locationTarget.ToList(), requiresSpecificExtract, numExtracts);

        // Remove the location from the available pool
        pool.Pool.Exploration.Locations.Remove(locationKey);

        return true;
    }

    /// <summary>
    ///     Get the number of times the player needs to exit
    /// </summary>
    /// <param name="config">Exploration config</param>
    /// <param name="requiresSpecificExtract">Is this a specific extract</param>
    /// <returns>Number of exit requirements</returns>
    protected int GetNumberOfExits(Exploration config, bool requiresSpecificExtract)
    {
        // Different max extract count when specific extract needed
        var exitTimesMax = requiresSpecificExtract
            ? config.MaximumExtractsWithSpecificExit
            : config.MaximumExtracts + 1;

        return randomUtil.RandInt(1, exitTimesMax);
    }

    /// <summary>
    ///     Filter a maps exits to just those for the desired side
    /// </summary>
    /// <param name="locationKey">Map id (e.g. factory4_day)</param>
    /// <param name="playerGroup">Pmc/Scav</param>
    /// <returns>List of Exit objects</returns>
    protected List<Exit>? GetLocationExitsForSide(string locationKey, PlayerGroup playerGroup)
    {
        var mapExtracts = databaseService.GetLocation(locationKey.ToLower())?.AllExtracts;

        return mapExtracts?.Where(exit => exit.Side == Enum.GetName(playerGroup)).ToList();
    }

    /// <summary>
    ///     Generate the initial available for finish condition
    /// </summary>
    /// <param name="quest">quest to add the condition to</param>
    /// <param name="locationInfo">LocationInfo object with the generated data</param>
    /// <returns>True if generated, false if not</returns>
    protected bool TryGenerateAvailableForFinish(RepeatableQuest quest, LocationInfo locationInfo)
    {
        // This should never be hit, this is here to shut the compiler up.
        if (quest.Conditions.AvailableForFinish?[0].Counter is null)
        {
            logger.Error("Counter is null, something has gone terribly wrong");
            return false;
        }

        // Lookup the location
        var location = repeatableQuestHelper.GetQuestLocationByMapId(locationInfo.LocationName.ToString());

        if (location is null)
        {
            // TODO - Localize me
            logger.Error($"Unable to get locationId for {locationInfo.LocationName}");
            return false;
        }

        var exitStatusCondition = new QuestConditionCounterCondition
        {
            Id = hashUtil.Generate(),
            DynamicLocale = true,
            Status = ["Survived"],
            ConditionType = "ExitStatus",
        };

        var locationCondition = new QuestConditionCounterCondition
        {
            Id = hashUtil.Generate(),
            DynamicLocale = true,
            Target = new ListOrT<string>(locationInfo.LocationTarget, null),
            ConditionType = "Location",
        };

        quest.Conditions.AvailableForFinish![0].Counter!.Id = hashUtil.Generate();
        quest.Conditions.AvailableForFinish![0].Counter!.Conditions =
        [
            exitStatusCondition,
            locationCondition,
        ];
        quest.Conditions.AvailableForFinish[0].Value = locationInfo.NumOfExtractsRequired;
        quest.Conditions.AvailableForFinish[0].Id = hashUtil.Generate();



        quest.Location = location;

        return true;
    }

    /// <summary>
    ///     Adds a specific extract requirement to the quest
    /// </summary>
    /// <param name="quest">quest to add it to</param>
    /// <param name="repeatableConfig">repeatable config</param>
    /// <param name="locationInfo">LocationInfo object with the generated data</param>
    /// <returns>True if generated, false if not</returns>
    protected bool TryGenerateSpecificExtractRequirement(RepeatableQuest quest, RepeatableQuestConfig repeatableConfig, LocationInfo locationInfo)
    {
        // Fetch extracts for the requested side
        var mapExits = GetLocationExitsForSide(locationInfo.LocationName.ToString(), repeatableConfig.Side);

        if (mapExits is null)
        {
            // TODO: Localize me
            logger.Error($"Unable to get location list for location {locationInfo.LocationName}");
            return false;
        }

        // Only get exits that have a greater than 0% chance to spawn
        var exitPool = mapExits.Where(exit => exit.Chance > 0).ToList();

        // Exclude exits with a requirement to leave (e.g. car extracts)
        var possibleExits = exitPool
            .Where(exit =>
                exit.PassageRequirement is not null
                || repeatableConfig.QuestConfig.Exploration.SpecificExits.PassageRequirementWhitelist.Contains(
                    "PassageRequirement"
                )
            )
            .ToList();

        if (possibleExits.Count == 0)
        {
            // TODO - Localize me!
            logger.Error(
                $"Unable to choose specific exit on map: {locationInfo.LocationName}, Possible exit pool was empty"
            );

            return false;
        }

        // Choose one of the exits we filtered above
        var chosenExit = randomUtil.DrawRandomFromList(possibleExits)[0];

        // Create a quest condition to leave raid via chosen exit
        var exitCondition = GenerateQuestConditionCounter(chosenExit);
        quest.Conditions.AvailableForFinish![0].Counter!.Conditions!.Add(exitCondition);

        return true;
    }

    /// <summary>
    ///     Exploration repeatable quests can specify a required extraction point.
    ///     This method creates the according object which will be appended to the conditions list
    /// </summary>
    /// <param name="exit">The exit name to generate the condition for</param>
    /// <returns>Exit condition</returns>
    protected QuestConditionCounterCondition GenerateQuestConditionCounter(Exit exit)
    {
        return new QuestConditionCounterCondition
        {
            Id = hashUtil.Generate(),
            DynamicLocale = true,
            ExitName = exit.Name,
            ConditionType = "ExitName",
        };
    }
}
