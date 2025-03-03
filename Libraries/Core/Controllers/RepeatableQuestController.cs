using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Profile;
using Core.Models.Eft.Quests;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Quests;
using Core.Models.Spt.Repeatable;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using Core.Utils.Collections;
using SptCommon.Annotations;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Controllers;

[Injectable]
public class RepeatableQuestController(
    ISptLogger<RepeatableQuestChangeRequest> _logger,
    TimeUtil _timeUtil,
    MathUtil _mathUtil,
    RandomUtil _randomUtil,
    HttpResponseUtil _httpResponseUtil,
    ProfileHelper _profileHelper,
    ProfileFixerService _profileFixerService,
    LocalisationService _localisationService,
    EventOutputHolder _eventOutputHolder,
    PaymentService _paymentService,
    RepeatableQuestGenerator _repeatableQuestGenerator,
    RepeatableQuestHelper _repeatableQuestHelper,
    QuestHelper _questHelper,
    DatabaseService _databaseService,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected QuestConfig _questConfig = _configServer.GetConfig<QuestConfig>();

    public ItemEventRouterResponse ChangeRepeatableQuest(PmcData pmcData, RepeatableQuestChangeRequest changeRequest,
        string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);

        var fullProfile = _profileHelper.GetFullProfile(sessionID);

        // Check for existing quest in (daily/weekly/scav arrays)
        var repeatables = GetRepeatableById(changeRequest.QuestId, pmcData);
        var questToReplace = repeatables.Quest;
        var repeatablesOfTypeInProfile = repeatables.RepeatableType;
        if (repeatables.RepeatableType is null || repeatables.Quest is null)
        {
            // Unable to find quest being replaced
            var message = _localisationService.GetText("quest-unable_to_find_repeatable_to_replace");
            _logger.Error(message);

            return _httpResponseUtil.AppendErrorToOutput(output, message);
        }

        // Subtype name of quest - daily/weekly/scav
        var repeatableTypeLower = repeatablesOfTypeInProfile.Name.ToLower();

        // Save for later standing loss calculation
        var replacedQuestTraderId = questToReplace.TraderId;

        // Update active quests to exclude the quest we're replacing
        repeatablesOfTypeInProfile.ActiveQuests = repeatablesOfTypeInProfile.ActiveQuests.Where(
                quest => quest.Id != changeRequest.QuestId
            )
            .ToList();

        // Save for later cost calculations
        var previousChangeRequirement = _cloner.Clone(
            repeatablesOfTypeInProfile.ChangeRequirement[changeRequest.QuestId]
        );

        // Delete the replaced quest change requirement data as we're going to add new data below
        repeatablesOfTypeInProfile.ChangeRequirement.Remove(changeRequest.QuestId);

        // Get config for this repeatable sub-type (daily/weekly/scav)
        var repeatableConfig = _questConfig.RepeatableQuests.FirstOrDefault(
            config => config.Name == repeatablesOfTypeInProfile.Name
        );

        // If the configuration dictates to replace with the same quest type, adjust the available quest types
        if (repeatableConfig?.KeepDailyQuestTypeOnReplacement is not null)
        {
            repeatableConfig.Types = [questToReplace.Type.ToString()];
        }

        // Generate meta-data for what type/levelrange of quests can be generated for player
        var allowedQuestTypes = GenerateQuestPool(repeatableConfig, pmcData.Info.Level);
        var newRepeatableQuest = AttemptToGenerateRepeatableQuest(
            sessionID,
            pmcData,
            allowedQuestTypes,
            repeatableConfig
        );
        if (newRepeatableQuest is null)
        {
            // Unable to find quest being replaced
            var message =
                $"Unable to generate repeatable quest of type: {repeatableTypeLower} to replace trader: {replacedQuestTraderId} quest: {changeRequest.QuestId}";
            _logger.Error(message);

            return _httpResponseUtil.AppendErrorToOutput(output, message);
        }

        // Add newly generated quest to daily/weekly/scav type array
        newRepeatableQuest.Side = repeatableConfig.Side;
        repeatablesOfTypeInProfile.ActiveQuests.Add(newRepeatableQuest);

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug(
                $"Removing: {repeatableConfig.Name} quest: {questToReplace.Id} from trader: {questToReplace.TraderId} as its been replaced"
            );
        }

        RemoveQuestFromProfile(fullProfile, questToReplace.Id);

        // Delete the replaced quest change requirement from profile
        CleanUpRepeatableChangeRequirements(repeatablesOfTypeInProfile, questToReplace.Id);

        // Add replacement quests change requirement data to profile
        repeatablesOfTypeInProfile.ChangeRequirement[newRepeatableQuest.Id] = new ChangeRequirement
        {
            ChangeCost = newRepeatableQuest.ChangeCost,
            ChangeStandingCost = _randomUtil.GetArrayValue([0, 0.01])
        };

        // Check if we should charge player for replacing quest
        var isFreeToReplace = UseFreeRefreshIfAvailable(
            fullProfile,
            repeatablesOfTypeInProfile,
            repeatableTypeLower
        );
        if (!isFreeToReplace)
        {
            // Reduce standing with trader for not doing their quest
            var traderOfReplacedQuest = pmcData.TradersInfo[replacedQuestTraderId];
            traderOfReplacedQuest.Standing -= previousChangeRequirement.ChangeStandingCost;

            var charismaBonus = _profileHelper.GetSkillFromProfile(pmcData, SkillTypes.Charisma)?.Progress ?? 0;
            foreach (var cost in previousChangeRequirement.ChangeCost)
            {
                // Not free, Charge player + appy charisma bonus to cost of replacement
                cost.Count = (int) Math.Truncate(cost.Count.Value * (1 - Math.Truncate(charismaBonus / 100) * 0.001));
                _paymentService.AddPaymentToOutput(pmcData, cost.TemplateId, cost.Count.Value, sessionID, output);
                if (output.Warnings.Count > 0)
                {
                    return output;
                }
            }
        }

        // Clone data before we send it to client
        var repeatableToChangeClone = _cloner.Clone(repeatablesOfTypeInProfile);

        // Purge inactive repeatables
        repeatableToChangeClone.InactiveQuests = [];

        // Nullguard
        output.ProfileChanges[sessionID].RepeatableQuests ??= [];

        // Update client output with new repeatable
        output.ProfileChanges[sessionID].RepeatableQuests.Add(repeatableToChangeClone);

        return output;
    }

    /**
     * Some accounts have access to free repeatable quest refreshes
     * Track the usage of them inside players profile
     * @param fullProfile Player profile
     * @param repeatableSubType Can be daily / weekly / scav repeatable
     * @param repeatableTypeName Subtype of repeatable quest: daily / weekly / scav
     * @returns Is the repeatable being replaced for free
     */
    protected bool UseFreeRefreshIfAvailable(SptProfile? fullProfile, PmcDataRepeatableQuest repeatableSubType,
        string repeatableTypeName)
    {
        // No free refreshes, exit early
        if (repeatableSubType.FreeChangesAvailable <= 0)
        {
            // Reset counter to 0
            repeatableSubType.FreeChangesAvailable = 0;

            return false;
        }

        // Only certain game versions have access to free refreshes
        var hasAccessToFreeRefreshSystem = _profileHelper.HasAccessToRepeatableFreeRefreshSystem(
            fullProfile.CharacterData.PmcData
        );

        // If the player has access and available refreshes:
        if (hasAccessToFreeRefreshSystem)
        {
            // Initialize/retrieve free refresh count for the desired subtype: daily/weekly
            fullProfile.SptData.FreeRepeatableRefreshUsedCount ??= new Dictionary<string, int>();
            var repeatableRefreshCounts = fullProfile.SptData.FreeRepeatableRefreshUsedCount;
            repeatableRefreshCounts.TryAdd(repeatableTypeName, 0); // Set to 0 if undefined

            // Increment the used count and decrement the available count.
            repeatableRefreshCounts[repeatableTypeName]++;
            repeatableSubType.FreeChangesAvailable--;

            return true;
        }

        return false;
    }

    /**
     * Clean up the repeatables `changeRequirement` dictionary of expired data
     * @param repeatablesOfTypeInProfile The repeatables that have the replaced and new quest
     * @param replacedQuestId Id of the replaced quest
     */
    protected void CleanUpRepeatableChangeRequirements(PmcDataRepeatableQuest repeatablesOfTypeInProfile,
        string replacedQuestId)
    {
        if (repeatablesOfTypeInProfile.ActiveQuests.Count == 1)
            // Only one repeatable quest being replaced (e.g. scav_daily), remove everything ready for new quest requirement to be added
            // Will assist in cleanup of existing profiles data
        {
            repeatablesOfTypeInProfile.ChangeRequirement.Clear();
        }
        else
            // Multiple active quests of this type (e.g. daily or weekly) are active, just remove the single replaced quest
        {
            repeatablesOfTypeInProfile.ChangeRequirement.Remove(replacedQuestId);
        }
    }

    protected RepeatableQuest? AttemptToGenerateRepeatableQuest(string sessionId, PmcData pmcData,
        QuestTypePool questTypePool, RepeatableQuestConfig repeatableConfig)
    {
        const int maxAttempts = 10;
        RepeatableQuest newRepeatableQuest = null;
        var attempts = 0;
        while (attempts < maxAttempts && questTypePool.Types.Count > 0)
        {
            newRepeatableQuest = _repeatableQuestGenerator.GenerateRepeatableQuest(
                sessionId,
                pmcData.Info.Level.Value,
                pmcData.TradersInfo,
                questTypePool,
                repeatableConfig
            );

            if (newRepeatableQuest is not null)
                // Successfully generated a quest, exit loop
            {
                break;
            }

            attempts++;
        }

        if (attempts > maxAttempts)
        {
            _logger.Error("We were stuck in repeatable quest generation. This should never happen. Please report");
        }

        return newRepeatableQuest;
    }

    protected void RemoveQuestFromProfile(SptProfile? fullProfile, string questToReplaceId)
    {
        // Find quest we're replacing in pmc profile quests array and remove it
        _questHelper.FindAndRemoveQuestFromArrayIfExists(questToReplaceId, fullProfile.CharacterData.PmcData.Quests);

        // Find quest we're replacing in scav profile quests array and remove it
        if (fullProfile.CharacterData.ScavData is not null)
        {
            _questHelper.FindAndRemoveQuestFromArrayIfExists(
                questToReplaceId,
                fullProfile.CharacterData.ScavData.Quests
            );
        }
    }

    /**
     * Find a repeatable (daily/weekly/scav) from a players profile by its id
     * @param questId Id of quest to find
     * @param pmcData Profile that contains quests to look through
     * @returns IGetRepeatableByIdResult
     */
    protected GetRepeatableByIdResult GetRepeatableById(string questId, PmcData pmcData)
    {
        foreach (var repeatablesInProfile in pmcData.RepeatableQuests)
        {
            // Check for existing quest in (daily/weekly/scav arrays)
            var questToReplace =
                repeatablesInProfile.ActiveQuests.FirstOrDefault(repeatable => repeatable.Id == questId);
            if (questToReplace is null)
                // Not found, skip to next repeatable sub-type
            {
                continue;
            }

            return new GetRepeatableByIdResult
            {
                Quest = questToReplace,
                RepeatableType = repeatablesInProfile
            };
        }

        return null;
    }

    public List<PmcDataRepeatableQuest> GetClientRepeatableQuests(string sessionID)
    {
        var returnData = new List<PmcDataRepeatableQuest>();
        var fullProfile = _profileHelper.GetFullProfile(sessionID);
        var pmcData = fullProfile.CharacterData.PmcData;
        var currentTime = _timeUtil.GetTimeStamp();

        // Daily / weekly / Daily_Savage
        foreach (var repeatableConfig in _questConfig.RepeatableQuests)
        {
            // Get daily/weekly data from profile, add empty object if missing
            var generatedRepeatables = GetRepeatableQuestSubTypeFromProfile(repeatableConfig, pmcData);
            var repeatableTypeLower = repeatableConfig.Name.ToLower();

            var canAccessRepeatables = CanProfileAccessRepeatableQuests(repeatableConfig, pmcData);
            if (!canAccessRepeatables)
                // Don't send any repeatables, even existing ones
            {
                continue;
            }

            // Existing repeatables are still valid, add to return data and move to next sub-type
            if (currentTime < generatedRepeatables.EndTime - 1)
            {
                returnData.Add(generatedRepeatables);

                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"[Quest Check] {repeatableTypeLower} quests are still valid.");
                }

                continue;
            }

            // Current time is past expiry time

            // Set endtime to be now + new duration
            generatedRepeatables.EndTime = currentTime + repeatableConfig.ResetTime;
            generatedRepeatables.InactiveQuests = [];
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Generating new {repeatableTypeLower}");
            }

            // Put old quests to inactive (this is required since only then the client makes them fail due to non-completion)
            // Also need to push them to the "inactiveQuests" list since we need to remove them from offraidData.profile.Quests
            // after a raid (the client seems to keep quests internally and we want to get rid of old repeatable quests)
            // and remove them from the PMC's Quests and RepeatableQuests[i].activeQuests
            ProcessExpiredQuests(generatedRepeatables, pmcData);

            // Create dynamic quest pool to avoid generating duplicates
            var questTypePool = GenerateQuestPool(repeatableConfig, pmcData.Info.Level);

            // Add repeatable quests of this loops sub-type (daily/weekly)
            for (var i = 0; i < GetQuestCount(repeatableConfig, pmcData); i++)
            {
                var quest = new RepeatableQuest();
                var lifeline = 0;
                while (quest?.Id == null && questTypePool.Types.Count > 0)
                {
                    quest = _repeatableQuestGenerator.GenerateRepeatableQuest(
                        sessionID,
                        pmcData.Info.Level ?? 0,
                        pmcData.TradersInfo,
                        questTypePool,
                        repeatableConfig
                    );
                    lifeline++;
                    if (lifeline > 10)
                    {
                        _logger.Error(
                            "We were stuck in repeatable quest generation. This should never happen. Please report"
                        );

                        break;
                    }
                }

                // check if there are no more quest types available
                if (questTypePool.Types.Count == 0)
                {
                    break;
                }

                quest.Side = repeatableConfig.Side;
                generatedRepeatables.ActiveQuests.Add(quest);
            }

            // Nullguard
            fullProfile.SptData.FreeRepeatableRefreshUsedCount ??= new Dictionary<string, int>();

            // Reset players free quest count for this repeatable sub-type as we're generating new repeatables for this group (daily/weekly)
            fullProfile.SptData.FreeRepeatableRefreshUsedCount[repeatableTypeLower] = 0;

            // Create stupid redundant change requirements from quest data
            generatedRepeatables.ChangeRequirement = new Dictionary<string, ChangeRequirement>();
            foreach (var quest in generatedRepeatables.ActiveQuests)
            {
                generatedRepeatables.ChangeRequirement.TryAdd(
                    quest.Id,
                    new ChangeRequirement
                    {
                        ChangeCost = quest.ChangeCost,
                        ChangeStandingCost = _randomUtil.GetArrayValue([0, 0.01]) // Randomise standing loss to replace
                    }
                );
            }

            // Reset free repeatable values in player profile to defaults
            generatedRepeatables.FreeChanges = repeatableConfig.FreeChanges;
            generatedRepeatables.FreeChangesAvailable = repeatableConfig.FreeChanges;

            returnData.Add(
                new PmcDataRepeatableQuest
                {
                    Id = repeatableConfig.Id,
                    Name = generatedRepeatables.Name,
                    EndTime = generatedRepeatables.EndTime,
                    ActiveQuests = generatedRepeatables.ActiveQuests,
                    InactiveQuests = generatedRepeatables.InactiveQuests,
                    ChangeRequirement = generatedRepeatables.ChangeRequirement,
                    FreeChanges = generatedRepeatables.FreeChanges,
                    FreeChangesAvailable = generatedRepeatables.FreeChanges
                }
            );
        }

        return returnData;
    }

    protected PmcDataRepeatableQuest GetRepeatableQuestSubTypeFromProfile(RepeatableQuestConfig repeatableConfig,
        PmcData pmcData)
    {
        // Get from profile, add if missing
        var repeatableQuestDetails = pmcData.RepeatableQuests.FirstOrDefault(
            repeatable => repeatable.Name == repeatableConfig.Name
        );
        if (repeatableQuestDetails is null)
        {
            // Not in profile, generate
            var hasAccess = _profileHelper.HasAccessToRepeatableFreeRefreshSystem(pmcData);
            repeatableQuestDetails = new PmcDataRepeatableQuest
            {
                Id = repeatableConfig.Id,
                Name = repeatableConfig.Name,
                ActiveQuests = [],
                InactiveQuests = [],
                EndTime = 0,
                FreeChanges = hasAccess ? repeatableConfig.FreeChanges : 0,
                FreeChangesAvailable = hasAccess ? repeatableConfig.FreeChangesAvailable : 0
            };

            // Add base object that holds repeatable data to profile
            pmcData.RepeatableQuests.Add(repeatableQuestDetails);
        }

        return repeatableQuestDetails;
    }

    protected bool CanProfileAccessRepeatableQuests(RepeatableQuestConfig repeatableConfig, PmcData pmcData)
    {
        // PMC and daily quests not unlocked yet
        if (repeatableConfig.Side == "Pmc" && !PlayerHasDailyPmcQuestsUnlocked(pmcData, repeatableConfig))
        {
            return false;
        }

        // Scav and daily quests not unlocked yet
        if (repeatableConfig.Side == "Scav" && !PlayerHasDailyScavQuestsUnlocked(pmcData))
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug("Daily scav quests still locked, Intel center not built");
            }

            return false;
        }

        return true;
    }

    /**
     * Does player have daily pmc quests unlocked
     * @param pmcData Player profile to check
     * @param repeatableConfig Config of daily type to check
     * @returns True if unlocked
     */
    protected static bool PlayerHasDailyPmcQuestsUnlocked(PmcData pmcData, RepeatableQuestConfig repeatableConfig)
    {
        return pmcData.Info.Level >= repeatableConfig.MinPlayerLevel;
    }

    /**
     * Does player have daily scav quests unlocked
     * @param pmcData Player profile to check
     * @returns True if unlocked
     */
    protected bool PlayerHasDailyScavQuestsUnlocked(PmcData pmcData)
    {
        return pmcData?.Hideout?.Areas?.FirstOrDefault(hideoutArea => hideoutArea.Type == HideoutAreas.INTEL_CENTER)
                   ?.Level >=
               1;
    }

    protected void ProcessExpiredQuests(PmcDataRepeatableQuest generatedRepeatables, PmcData pmcData)
    {
        var questsToKeep = new List<RepeatableQuest>();
        foreach (var activeQuest in generatedRepeatables.ActiveQuests)
        {
            var questStatusInProfile = pmcData.Quests.FirstOrDefault(quest => quest.QId == activeQuest.Id);
            if (questStatusInProfile is null)
            {
                continue;
            }

            // Keep finished quests in list so player can hand in
            if (questStatusInProfile.Status == QuestStatusEnum.AvailableForFinish)
            {
                questsToKeep.Add(activeQuest);
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug( // TODO: this shouldnt happen, doesnt on live
                        $"Keeping repeatable quest: {activeQuest.Id} in activeQuests since it is available to hand in"
                    );
                }

                continue;
            }

            // Clean up quest-related counters being left in profile
            _profileFixerService.RemoveDanglingConditionCounters(pmcData);

            // Remove expired quest from pmc.quest array
            pmcData.Quests = pmcData.Quests.Where(quest => quest.QId != activeQuest.Id).ToList();

            // Store in inactive array
            generatedRepeatables.InactiveQuests.Add(activeQuest);
        }

        generatedRepeatables.ActiveQuests = questsToKeep;
    }

    protected QuestTypePool GenerateQuestPool(RepeatableQuestConfig repeatableConfig, int? pmcLevel)
    {
        var questPool = CreateBaseQuestPool(repeatableConfig);

        // Get the allowed locations based on the PMC's level
        var locations = GetAllowedLocationsForPmcLevel(repeatableConfig.Locations, pmcLevel.Value);

        // Populate Exploration and Pickup quest locations
        foreach (var (location, value) in locations)
        {
            if (location != ELocationName.any)
            {
                questPool.Pool.Exploration.Locations[location] = value;
                questPool.Pool.Pickup.Locations[location] = value;
            }
        }

        // Add "any" to pickup quest pool
        questPool.Pool.Pickup.Locations[ELocationName.any] = ["any"];

        var eliminationConfig = _repeatableQuestHelper.GetEliminationConfigByPmcLevel(pmcLevel.Value, repeatableConfig);
        var targetsConfig = new ProbabilityObjectArray<string, BossInfo>(_mathUtil, _cloner, eliminationConfig.Targets);

        // Populate Elimination quest targets and their locations
        foreach (var targetKvP in targetsConfig)
            // Target is boss
        {
            if (targetKvP.Data.IsBoss.GetValueOrDefault(false))
            {
                questPool.Pool.Elimination.Targets.Add(
                    targetKvP.Key,
                    new TargetLocation
                    {
                        Locations = ["any"]
                    }
                );
            }
            else
            {
                // Non-boss targets
                var possibleLocations = locations.Keys;

                var allowedLocations =
                    targetKvP.Key == "Savage"
                        ? possibleLocations.Where(
                            location => location != ELocationName.laboratory
                        ) // Exclude labs for Savage targets.
                        : possibleLocations;

                questPool.Pool.Elimination.Targets[targetKvP.Key] = new TargetLocation
                {
                    Locations = allowedLocations.Select(x => x.ToString()).ToList()
                };
            }
        }

        return questPool;
    }

    protected QuestTypePool CreateBaseQuestPool(RepeatableQuestConfig repeatableConfig)
    {
        return new QuestTypePool
        {
            Types = _cloner.Clone(repeatableConfig.Types),
            Pool = new QuestPool
            {
                Exploration = new ExplorationPool
                {
                    Locations = new Dictionary<ELocationName, List<string>>()
                },
                Elimination = new EliminationPool
                {
                    Targets = new Dictionary<string, TargetLocation>()
                },
                Pickup = new ExplorationPool
                {
                    Locations = new Dictionary<ELocationName, List<string>>()
                }
            }
        };
    }

    protected Dictionary<ELocationName, List<string>> GetAllowedLocationsForPmcLevel(
        Dictionary<ELocationName, List<string>> locations, int pmcLevel)
    {
        var allowedLocation = new Dictionary<ELocationName, List<string>>();

        foreach (var (location, value) in locations)
        {
            var locationNames = new List<string>();
            foreach (var locationName in value)
            {
                if (IsPmcLevelAllowedOnLocation(locationName, pmcLevel))
                {
                    locationNames.Add(locationName);
                }
            }

            if (locationNames.Count > 0)
            {
                allowedLocation[location] = locationNames;
            }
        }

        return allowedLocation;
    }

    /**
     * Return true if the given pmcLevel is allowed on the given location
     * @param location The location name to check
     * @param pmcLevel The level of the pmc
     * @returns True if the given pmc level is allowed to access the given location
     */
    protected bool IsPmcLevelAllowedOnLocation(string location, int pmcLevel)
    {
        // All PMC levels are allowed for 'any' location requirement
        if (location == ELocationName.any.ToString())
        {
            return true;
        }

        var locationBase = _databaseService.GetLocation(location.ToLower())?.Base;
        if (locationBase is null)
        {
            return true;
        }

        return pmcLevel <= locationBase.RequiredPlayerLevelMax && pmcLevel >= locationBase.RequiredPlayerLevelMin;
    }

    /// <summary>
    ///     Get count of repeatable quests profile should have access to
    /// </summary>
    /// <param name="repeatableConfig"></param>
    /// <param name="pmcData">Player profile</param>
    /// <returns>Quest count</returns>
    protected int GetQuestCount(RepeatableQuestConfig repeatableConfig, PmcData pmcData)
    {
        var questCount = repeatableConfig.NumQuests.GetValueOrDefault(0);
        if (questCount == 0)
        {
            _logger.Warning($"Repeatable {repeatableConfig.Name} quests have a count of 0");
        }

        // Add elite bonus to daily quests
        if (string.Equals(repeatableConfig.Name, "daily", StringComparison.OrdinalIgnoreCase) && _profileHelper.HasEliteSkillLevel(SkillTypes.Charisma, pmcData))
            // Elite charisma skill gives extra daily quest(s)
        {
            questCount += _databaseService
                .GetGlobals()
                .Configuration
                .SkillsSettings
                .Charisma
                .BonusSettings
                .EliteBonusSettings
                .RepeatableQuestExtraCount
                .GetValueOrDefault(0);
        }

        // Prestige level 2 gives additional daily and weekly
        // do the logic for all other than "daily_savage"
        // use bigger than or equal incase modders add more
        if (repeatableConfig.Name.ToLower() != "daily_savage" && pmcData.Info.PrestigeLevel >= 2)
        {
            questCount++;
        }

        return questCount;
    }
}
