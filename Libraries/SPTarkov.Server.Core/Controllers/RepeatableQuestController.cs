using System.Collections.Frozen;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Generators.RepeatableQuestGeneration;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Quests;
using SPTarkov.Server.Core.Models.Spt.Repeatable;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Server.Core.Utils.Collections;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class RepeatableQuestController(
    ISptLogger<RepeatableQuestChangeRequest> logger,
    EliminationQuestGenerator eliminationQuestGenerator,
    CompletionQuestGenerator completionQuestGenerator,
    ExplorationQuestGenerator explorationQuestGenerator,
    PickupQuestGenerator pickupQuestGenerator,
    TimeUtil timeUtil,
    RandomUtil randomUtil,
    HttpResponseUtil httpResponseUtil,
    ProfileHelper profileHelper,
    ProfileFixerService profileFixerService,
    ServerLocalisationService serverLocalisationService,
    EventOutputHolder eventOutputHolder,
    PaymentService paymentService,
    RepeatableQuestHelper repeatableQuestHelper,
    QuestHelper questHelper,
    DatabaseService databaseService,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected static readonly FrozenSet<string> _questTypes = ["PickUp", "Exploration", "Elimination"];
    protected readonly QuestConfig QuestConfig = configServer.GetConfig<QuestConfig>();

    /// <summary>
    ///     Handle the client accepting a repeatable quest and starting it
    ///     Send starting rewards if any to player and
    ///     Send start notification if any to player
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="acceptedQuest">Repeatable quest accepted</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse AcceptRepeatableQuest(PmcData pmcData, AcceptQuestRequestData acceptedQuest, MongoId sessionID)
    {
        // Create and store quest status object inside player profile
        var newRepeatableQuest = questHelper.GetQuestReadyForProfile(pmcData, QuestStatusEnum.Started, acceptedQuest);
        pmcData.Quests.Add(newRepeatableQuest);

        // Look for the generated quest cache in profile.RepeatableQuests
        var repeatableQuestProfile = GetRepeatableQuestFromProfile(pmcData, acceptedQuest.QuestId);
        if (repeatableQuestProfile is null)
        {
            logger.Error(
                serverLocalisationService.GetText("repeatable-accepted_repeatable_quest_not_found_in_active_quests", acceptedQuest.QuestId)
            );

            throw new Exception(serverLocalisationService.GetText("repeatable-unable_to_accept_quest_see_log"));
        }

        // Some scav quests need to be added to scav profile for them to show up in-raid
        if (repeatableQuestProfile.Side == "Scav" && _questTypes.Contains(repeatableQuestProfile.Type.ToString()))
        {
            var fullProfile = profileHelper.GetFullProfile(sessionID);

            fullProfile.CharacterData.ScavData.Quests ??= [];
            fullProfile.CharacterData.ScavData.Quests.Add(newRepeatableQuest);
        }

        var response = eventOutputHolder.GetOutput(sessionID);

        return response;
    }

    /// <summary>
    ///     Handle RepeatableQuestChange event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="changeRequest">Change quest request</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse ChangeRepeatableQuest(PmcData pmcData, RepeatableQuestChangeRequest changeRequest, MongoId sessionID)
    {
        var output = eventOutputHolder.GetOutput(sessionID);

        var fullProfile = profileHelper.GetFullProfile(sessionID);

        // Check for existing quest in (daily/weekly/scav arrays)
        var repeatables = GetRepeatableById(changeRequest.QuestId, pmcData);
        var questToReplace = repeatables.Quest;
        var repeatablesOfTypeInProfile = repeatables.RepeatableType;
        if (repeatables.RepeatableType is null || repeatables.Quest is null)
        {
            // Unable to find quest being replaced
            var message = serverLocalisationService.GetText("quest-unable_to_find_repeatable_to_replace");
            logger.Error(message);

            return httpResponseUtil.AppendErrorToOutput(output, message);
        }

        // Subtype name of quest - daily/weekly/scav
        var repeatableTypeLower = repeatablesOfTypeInProfile.Name.ToLowerInvariant();

        // Save for later standing loss calculation
        var replacedQuestTraderId = questToReplace.TraderId;

        // Update active quests to exclude the quest we're replacing
        repeatablesOfTypeInProfile.ActiveQuests = repeatablesOfTypeInProfile
            .ActiveQuests.Where(quest => quest.Id != changeRequest.QuestId)
            .ToList();

        // Save for later cost calculations
        var previousChangeRequirement = cloner.Clone(repeatablesOfTypeInProfile.ChangeRequirement[changeRequest.QuestId]);

        // Delete the replaced quest change requirement data as we're going to add new data below
        repeatablesOfTypeInProfile.ChangeRequirement.Remove(changeRequest.QuestId);

        // Get config for this repeatable subtype (daily/weekly/scav)
        var repeatableConfig = QuestConfig.RepeatableQuests.FirstOrDefault(config => config.Name == repeatablesOfTypeInProfile.Name);

        // If the configuration dictates to replace with the same quest type, adjust the available quest types
        if (repeatableConfig?.KeepDailyQuestTypeOnReplacement is not null)
        {
            repeatableConfig.Types = [questToReplace.Type.ToString()];
        }

        // Generate meta-data for what type/level range of quests can be generated for player
        var allowedQuestTypes = GenerateQuestPool(repeatableConfig, pmcData.Info.Level.GetValueOrDefault(1));
        var newRepeatableQuest = AttemptToGenerateRepeatableQuest(sessionID, pmcData, allowedQuestTypes, repeatableConfig);
        if (newRepeatableQuest is null)
        {
            // Unable to find quest being replaced
            var message =
                $"Unable to generate repeatable quest of type: {repeatableTypeLower} to replace trader: {replacedQuestTraderId} quest: {changeRequest.QuestId}";
            logger.Error(message);

            return httpResponseUtil.AppendErrorToOutput(output, message);
        }

        // Add newly generated quest to daily/weekly/scav type array
        newRepeatableQuest.Side = Enum.GetName(repeatableConfig.Side);
        repeatablesOfTypeInProfile.ActiveQuests.Add(newRepeatableQuest);

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug(
                $"Removing: {repeatableConfig.Name} quest: {questToReplace.Id} from trader: {questToReplace.TraderId} as its been replaced"
            );
        }

        // Remove the replaced quest from profile
        RemoveQuestFromProfile(fullProfile, questToReplace.Id);

        // Delete the replaced quest change requirement from profile
        CleanUpRepeatableChangeRequirements(repeatablesOfTypeInProfile, questToReplace.Id);

        // Add replacement quests change requirement data to profile
        repeatablesOfTypeInProfile.ChangeRequirement[newRepeatableQuest.Id] = new ChangeRequirement
        {
            ChangeCost = newRepeatableQuest.ChangeCost,
            ChangeStandingCost = randomUtil.GetArrayValue(repeatableConfig.StandingChangeCost),
        };

        // Check if we should charge player for replacing quest
        var isFreeToReplace = UseFreeRefreshIfAvailable(fullProfile, repeatablesOfTypeInProfile, repeatableTypeLower);
        if (!isFreeToReplace)
        {
            // Reduce standing with trader for not doing their quest
            var traderOfReplacedQuest = pmcData.TradersInfo[replacedQuestTraderId];
            traderOfReplacedQuest.Standing -= previousChangeRequirement.ChangeStandingCost;

            var charismaBonus = pmcData.GetSkillFromProfile(SkillTypes.Charisma)?.Progress ?? 0;
            foreach (var cost in previousChangeRequirement.ChangeCost)
            {
                // Not free, Charge player + apply charisma bonus to cost of replacement
                cost.Count = (int)Math.Truncate(cost.Count.Value * (1 - (Math.Truncate(charismaBonus / 100) * 0.001)));
                paymentService.AddPaymentToOutput(pmcData, cost.TemplateId, cost.Count.Value, sessionID, output);
                if (output.Warnings.Count > 0)
                {
                    return output;
                }
            }
        }

        // Clone data before we send it to client
        var repeatableToChangeClone = cloner.Clone(repeatablesOfTypeInProfile);

        // Purge inactive repeatables
        repeatableToChangeClone.InactiveQuests = [];

        // Update client output with new repeatable
        output.ProfileChanges[sessionID].RepeatableQuests ??= [];
        output.ProfileChanges[sessionID].RepeatableQuests.Add(repeatableToChangeClone);

        return output;
    }

    /// <summary>
    ///     Look for an accepted quest inside player profile, return quest that matches
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="questId">Quest id to return</param>
    /// <returns>RepeatableQuest</returns>
    protected RepeatableQuest? GetRepeatableQuestFromProfile(PmcData pmcData, MongoId questId)
    {
        foreach (var repeatableQuest in pmcData.RepeatableQuests)
        {
            var matchingQuest = repeatableQuest.ActiveQuests?.FirstOrDefault(x => x.Id == questId);
            if (matchingQuest is null)
            {
                // No daily/weekly/scav repeatable, skip over to next subtype
                continue;
            }

            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Accepted repeatable quest: {questId} from: {repeatableQuest.Name}");
            }

            matchingQuest.SptRepatableGroupName = repeatableQuest.Name;

            return matchingQuest;
        }

        return null;
    }

    /// <summary>
    ///     Some accounts have access to free repeatable quest refreshes
    ///     Track the usage of them inside players profile
    /// </summary>
    /// <param name="fullProfile">Full player profile</param>
    /// <param name="repeatableSubType">Can be daily / weekly / scav repeatable</param>
    /// <param name="repeatableTypeName">Subtype of repeatable quest: daily / weekly / scav</param>
    /// <returns>Is the repeatable being replaced for free</returns>
    protected bool UseFreeRefreshIfAvailable(SptProfile? fullProfile, PmcDataRepeatableQuest repeatableSubType, string repeatableTypeName)
    {
        // No free refreshes, exit early
        if (repeatableSubType.FreeChangesAvailable <= 0)
        {
            // Reset counter to 0
            repeatableSubType.FreeChangesAvailable = 0;

            return false;
        }

        // Only certain game versions have access to free refreshes
        var hasAccessToFreeRefreshSystem = profileHelper.HasAccessToRepeatableFreeRefreshSystem(fullProfile.CharacterData.PmcData);

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

    /// <summary>
    ///     Clean up the repeatables `changeRequirement` dictionary of expired data
    /// </summary>
    /// <param name="repeatablesOfTypeInProfile">repeatables that have the replaced and new quest</param>
    /// <param name="replacedQuestId">Id of the replaced quest</param>
    protected void CleanUpRepeatableChangeRequirements(PmcDataRepeatableQuest repeatablesOfTypeInProfile, string replacedQuestId)
    {
        if (repeatablesOfTypeInProfile.ActiveQuests.Count == 1)
        // Only one repeatable quest being replaced (e.g. scav_daily), remove everything ready for new quest requirement to be added
        // Will assist in cleanup of existing profiles data
        {
            repeatablesOfTypeInProfile.ChangeRequirement.Clear();

            return;
        }

        // Multiple active quests of this type (e.g. daily or weekly) are active, just remove the single replaced quest
        repeatablesOfTypeInProfile.ChangeRequirement.Remove(replacedQuestId);
    }

    /// <summary>
    ///     Generate a repeatable quest
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="questTypePool">What type/level range of quests can be generated for player</param>
    /// <param name="repeatableConfig">Config for the quest type to generate</param>
    /// <returns></returns>
    protected RepeatableQuest? AttemptToGenerateRepeatableQuest(
        MongoId sessionId,
        PmcData pmcData,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
    )
    {
        const int maxAttempts = 10;
        RepeatableQuest? newRepeatableQuest = null;
        var attempts = 0;
        while (attempts < maxAttempts && questTypePool.Types.Count > 0)
        {
            newRepeatableQuest = PickAndGenerateRandomRepeatableQuest(
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
            logger.Error(serverLocalisationService.GetText("quest-repeatable_generation_failed_please_report", attempts));
        }

        return newRepeatableQuest;
    }

    /// <summary>
    ///     This method is called by /GetClientRepeatableQuests/ and creates one element of quest type format (see
    ///     assets/database/templates/repeatableQuests.json).
    ///     It randomly draws a quest type (currently Elimination, Completion or Exploration) as well as a trader who is
    ///     providing the quest
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcLevel">Player's level for requested items and reward generation</param>
    /// <param name="pmcTraderInfo">Players trader standing/rep levels</param>
    /// <param name="questTypePool">Possible quest types pool</param>
    /// <param name="repeatableConfig">Repeatable quest config</param>
    /// <returns>RepeatableQuest</returns>
    public RepeatableQuest? PickAndGenerateRandomRepeatableQuest(
        MongoId sessionId,
        int pmcLevel,
        Dictionary<MongoId, TraderInfo> pmcTraderInfo,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
    )
    {
        var questType = randomUtil.DrawRandomFromList(questTypePool.Types).First();
        var traderId = DrawRandomTraderId(pmcTraderInfo, questType, repeatableConfig);

        if (traderId.IsEmpty)
        {
            logger.Error(serverLocalisationService.GetText("repeatable-unable_to_find_trader_in_pool"));

            return null;
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Generating operation task type: {questType} for {traderId}");
        }

        return questType switch
        {
            "Elimination" => eliminationQuestGenerator.Generate(sessionId, pmcLevel, traderId, questTypePool, repeatableConfig),
            "Completion" => completionQuestGenerator.Generate(sessionId, pmcLevel, traderId, questTypePool, repeatableConfig),
            "Exploration" => explorationQuestGenerator.Generate(sessionId, pmcLevel, traderId, questTypePool, repeatableConfig),
            "Pickup" => pickupQuestGenerator.Generate(sessionId, pmcLevel, traderId, questTypePool, repeatableConfig),
            _ => null,
        };
    }

    /// <summary>
    ///     Draws a random trader to assign a repeatable task to
    /// </summary>
    /// <param name="traderInfos">Trader info</param>
    /// <param name="questType">Type of quest</param>
    /// <param name="repeatableConfig">Repeatable config</param>
    /// <returns>Randomly selected traderId</returns>
    protected MongoId DrawRandomTraderId(
        Dictionary<MongoId, TraderInfo> traderInfos,
        string questType,
        RepeatableQuestConfig repeatableConfig
    )
    {
        // Get traders from whitelist and filter by quest type availability
        var traders = repeatableConfig
            .TraderWhitelist.Where(whitelist => whitelist.QuestTypes.Contains(questType))
            .Select(x => x.TraderId)
            // filter out locked traders
            .Where(mongoId => traderInfos[mongoId].Unlocked.GetValueOrDefault(false))
            .ToList();

        return randomUtil.DrawRandomFromList(traders).FirstOrDefault();
    }

    /// <summary>
    ///     Remove the provided quest from pmc and scav character profiles
    /// </summary>
    /// <param name="fullProfile">Profile to remove quest from</param>
    /// <param name="questToReplaceId">Quest id to remove from profile</param>
    protected void RemoveQuestFromProfile(SptProfile fullProfile, MongoId questToReplaceId)
    {
        // Find quest we're replacing in pmc profile quests array and remove it
        questHelper.FindAndRemoveQuestFromArrayIfExists(questToReplaceId, fullProfile.CharacterData.PmcData.Quests);

        // Look for and remove quest we're replacing in scav profile too
        if (fullProfile.CharacterData.ScavData is not null)
        {
            questHelper.FindAndRemoveQuestFromArrayIfExists(questToReplaceId, fullProfile.CharacterData.ScavData.Quests);
        }
    }

    /// <summary>
    ///     Find a repeatable (daily/weekly/scav) from a players profile by its id
    /// </summary>
    /// <param name="questId">Id of quest to find</param>
    /// <param name="pmcData">Profile that contains quests to look through</param>
    /// <returns></returns>
    protected GetRepeatableByIdResult? GetRepeatableById(MongoId questId, PmcData pmcData)
    {
        foreach (var repeatablesInProfile in pmcData.RepeatableQuests)
        {
            // Check for existing quest in (daily/weekly/scav arrays)
            var questToReplace = repeatablesInProfile.ActiveQuests?.FirstOrDefault(repeatable => repeatable.Id == questId);
            if (questToReplace is null)
            // Not found, skip to next repeatable subtype
            {
                continue;
            }

            return new GetRepeatableByIdResult { Quest = questToReplace, RepeatableType = repeatablesInProfile };
        }

        return null;
    }

    /// <summary>
    ///     Handle client/repeatableQuests/activityPeriods
    ///     Returns an array of objects in the format of repeatable quests to the client.
    ///     repeatableQuestObject = {
    ///     *id: Unique Id,
    ///     name: "Daily",
    ///     endTime: the time when the quests expire
    ///     activeQuests: currently available quests in an array. Each element of quest type format(see assets/ database / templates / repeatableQuests.json).
    ///     inactiveQuests: the quests which were previously active(required by client to fail them if they are not completed)
    ///     }
    ///     The method checks if the player level requirement for repeatable quests(e.g.daily lvl5, weekly lvl15) is met and if the previously active quests
    ///     are still valid.This ischecked by endTime persisted in profile accordning to the resetTime configured for each repeatable kind(daily, weekly)
    ///     in QuestCondig.js
    ///     If the condition is met, new repeatableQuests are created, old quests(which are persisted in the profile.RepeatableQuests[i].activeQuests) are
    ///     moved to profile.RepeatableQuests[i].inactiveQuests.This memory is required to get rid of old repeatable quest data in the profile, otherwise
    ///     they'll litter the profile's Quests field.
    ///     (if the are on "Succeed" but not "Completed" we keep them, to allow the player to complete them and get the rewards)
    ///     The new quests generated are again persisted in profile.RepeatableQuests
    /// </summary>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>Array of repeatable quests</returns>
    public List<PmcDataRepeatableQuest> GetClientRepeatableQuests(MongoId sessionID)
    {
        var returnData = new List<PmcDataRepeatableQuest>();
        var fullProfile = profileHelper.GetFullProfile(sessionID);
        var pmcData = fullProfile.CharacterData.PmcData;
        var currentTime = timeUtil.GetTimeStamp();

        // Daily / weekly / Daily_Savage
        foreach (var repeatableConfig in QuestConfig.RepeatableQuests)
        {
            // Get daily/weekly data from profile, add empty object if missing
            var generatedRepeatables = GetRepeatableQuestSubTypeFromProfile(repeatableConfig, pmcData);
            var repeatableTypeLower = repeatableConfig.Name.ToLowerInvariant();

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

                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"[Quest Check] {repeatableTypeLower} quests are still valid.");
                }

                continue;
            }

            // Current time is past expiry time

            // Set endtime to be now + new duration
            generatedRepeatables.EndTime = currentTime + repeatableConfig.ResetTime;
            generatedRepeatables.InactiveQuests = [];
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Generating new {repeatableTypeLower}");
            }

            // Put old quests to inactive (this is required since only then the client makes them fail due to non-completion)
            // Also need to push them to the "inactiveQuests" list since we need to remove them from offraidData.profile.Quests
            // after a raid (the client seems to keep quests internally and we want to get rid of old repeatable quests)
            // and remove them from the PMC's Quests and RepeatableQuests[i].activeQuests
            ProcessExpiredQuests(generatedRepeatables, pmcData);

            // Create dynamic quest pool to avoid generating duplicates
            var questTypePool = GenerateQuestPool(repeatableConfig, pmcData.Info.Level.GetValueOrDefault(1));

            // Add repeatable quests of this loops sub-type (daily/weekly)
            for (var i = 0; i < GetQuestCount(repeatableConfig, fullProfile); i++)
            {
                RepeatableQuest? quest;

                // The first 3 initial dailies generated need to be of separate types
                if (repeatableConfig.Name == "Daily" && i < 3)
                {
                    var questType = i switch
                    {
                        0 => "Elimination",
                        1 => "Completion",
                        2 => "Exploration",
                        _ => null,
                    };

                    if (questType is null)
                    {
                        logger.Error(
                            $"Repeatable index: `{i}` is out of range for valid quest types, this should never be hit, report it."
                        );
                        continue;
                    }

                    var traderId = DrawRandomTraderId(pmcData.TradersInfo, questType, repeatableConfig);

                    quest = i switch
                    {
                        0 => eliminationQuestGenerator.Generate(
                            sessionID,
                            pmcData.Info.Level ?? 0,
                            traderId,
                            questTypePool,
                            repeatableConfig
                        ),
                        1 => completionQuestGenerator.Generate(
                            sessionID,
                            pmcData.Info.Level ?? 0,
                            traderId,
                            questTypePool,
                            repeatableConfig
                        ),
                        2 => explorationQuestGenerator.Generate(
                            sessionID,
                            pmcData.Info.Level ?? 0,
                            traderId,
                            questTypePool,
                            repeatableConfig
                        ),
                        _ => null,
                    };
                }
                // Not part of the first 3 dailies generated or not a daily task, generate a random type of task
                else
                {
                    quest = TryGenerateRandomRepeatable(sessionID, pmcData, questTypePool, repeatableConfig);
                }

                // check if there are no more quest types available
                if (questTypePool.Types.Count == 0)
                {
                    break;
                }

                quest.Side = Enum.GetName(repeatableConfig.Side);
                generatedRepeatables.ActiveQuests.Add(quest);
            }

            // Nullguard
            fullProfile.SptData.FreeRepeatableRefreshUsedCount ??= new Dictionary<string, int>();

            // Reset players free quest count for this repeatable sub-type as we're generating new repeatables for this group (daily/weekly)
            fullProfile.SptData.FreeRepeatableRefreshUsedCount[repeatableTypeLower] = 0;

            // Create stupid redundant change requirements from quest data
            generatedRepeatables.ChangeRequirement = new Dictionary<MongoId, ChangeRequirement?>();
            foreach (var quest in generatedRepeatables.ActiveQuests)
            {
                generatedRepeatables.ChangeRequirement.TryAdd(
                    quest.Id,
                    new ChangeRequirement
                    {
                        ChangeCost = quest.ChangeCost,
                        ChangeStandingCost = randomUtil.GetArrayValue(repeatableConfig.StandingChangeCost), // Randomise standing loss to replace
                    }
                );
            }

            // Reset free repeatable values in player profile to defaults
            generatedRepeatables.FreeChangesAvailable = generatedRepeatables.FreeChanges;

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
                    FreeChangesAvailable = generatedRepeatables.FreeChangesAvailable,
                }
            );
        }

        return returnData;
    }

    /// <summary>
    ///     Try to generate a repeatable task at random
    /// </summary>
    /// <param name="sessionId">sessionId to generate for</param>
    /// <param name="pmcData">Pmc profile to add the quest to</param>
    /// <param name="questTypePool">Pool of quests to pick from</param>
    /// <param name="repeatableConfig">Repeatable quest config</param>
    /// <returns></returns>
    protected RepeatableQuest? TryGenerateRandomRepeatable(
        MongoId sessionId,
        PmcData pmcData,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
    )
    {
        RepeatableQuest? quest = null;
        var lifeline = 0;

        while (quest?.Id == null && questTypePool.Types.Count > 0)
        {
            quest = PickAndGenerateRandomRepeatableQuest(
                sessionId,
                pmcData.Info.Level ?? 0,
                pmcData.TradersInfo,
                questTypePool,
                repeatableConfig
            );

            lifeline++;
            if (lifeline <= 10)
            {
                continue;
            }

            logger.Error("We were stuck in repeatable quest generation. This should never happen. Please report");

            break;
        }

        return quest;
    }

    /// <summary>
    ///     Get repeatable quest data from profile from name (daily/weekly), creates base repeatable quest object if none exists
    /// </summary>
    /// <param name="repeatableConfig">daily/weekly config</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <returns>PmcDataRepeatableQuest</returns>
    protected PmcDataRepeatableQuest GetRepeatableQuestSubTypeFromProfile(RepeatableQuestConfig repeatableConfig, PmcData pmcData)
    {
        // Get from profile, add if missing
        var repeatableQuestDetails = pmcData.RepeatableQuests.FirstOrDefault(repeatable => repeatable.Name == repeatableConfig.Name);
        var hasAccess = profileHelper.HasAccessToRepeatableFreeRefreshSystem(pmcData);

        if (repeatableQuestDetails is null)
        {
            // Not in profile, generate
            repeatableQuestDetails = new PmcDataRepeatableQuest
            {
                Id = repeatableConfig.Id,
                Name = repeatableConfig.Name,
                ActiveQuests = [],
                InactiveQuests = [],
                EndTime = 0,
                FreeChanges = hasAccess ? repeatableConfig.FreeChanges : 0,
                FreeChangesAvailable = hasAccess ? repeatableConfig.FreeChangesAvailable : 0,
            };

            // Add base object that holds repeatable data to profile
            pmcData.RepeatableQuests.Add(repeatableQuestDetails);
        }

        // There is a chance an invalid number of free changes was assigned to the profile in earlier versions
        // reset the number if the user doesn't have access
        if (!hasAccess)
        {
            repeatableQuestDetails.FreeChanges = 0;
            repeatableQuestDetails.FreeChangesAvailable = 0;
        }

        return repeatableQuestDetails;
    }

    /// <summary>
    ///     Check if a repeatable quest type (daily/weekly) is active for the given profile
    /// </summary>
    /// <param name="repeatableConfig">Repeatable quest config</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <returns>True if profile has access to repeatables</returns>
    protected bool CanProfileAccessRepeatableQuests(RepeatableQuestConfig repeatableConfig, PmcData pmcData)
    {
        // PMC and daily quests not unlocked yet
        if (repeatableConfig.Side == PlayerGroup.Pmc && !PlayerHasDailyPmcQuestsUnlocked(pmcData, repeatableConfig))
        {
            return false;
        }

        // Scav and daily quests not unlocked yet
        if (repeatableConfig.Side == PlayerGroup.Scav && !PlayerHasDailyScavQuestsUnlocked(pmcData))
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug("Daily scav quests still locked, Intel center not built");
            }

            return false;
        }

        return true;
    }

    /// <summary>
    ///     Does player have daily pmc quests unlocked
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="repeatableConfig">Config of daily type to check</param>
    /// <returns>True if unlocked</returns>
    protected static bool PlayerHasDailyPmcQuestsUnlocked(PmcData pmcData, RepeatableQuestConfig repeatableConfig)
    {
        return pmcData.Info.Level >= repeatableConfig.MinPlayerLevel;
    }

    /// <summary>
    ///     Does player have daily scav quests unlocked
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <returns>True if unlocked</returns>
    protected bool PlayerHasDailyScavQuestsUnlocked(PmcData pmcData)
    {
        return pmcData.Hideout?.Areas?.FirstOrDefault(hideoutArea => hideoutArea.Type == HideoutAreas.IntelligenceCenter)?.Level >= 1;
    }

    /// <summary>
    ///     Expire quests and replace expired quests with ready-to-hand-in quests inside generatedRepeatables.activeQuests
    /// </summary>
    /// <param name="generatedRepeatables">Repeatables to process (daily/weekly)</param>
    /// <param name="pmcData">Players PMC profile</param>
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
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug( // TODO: this shouldn't happen, doesn't on live
                        $"Keeping repeatable quest: {activeQuest.Id} in activeQuests since it is available to hand in"
                    );
                }

                continue;
            }

            // Clean up quest-related counters being left in profile
            profileFixerService.RemoveDanglingConditionCounters(pmcData);

            // Remove expired quest from pmc.quest array
            pmcData.Quests = pmcData.Quests.Where(quest => quest.QId != activeQuest.Id).ToList();

            // Store in inactive array
            generatedRepeatables.InactiveQuests.Add(activeQuest);
        }

        generatedRepeatables.ActiveQuests = questsToKeep;
    }

    /// <summary>
    ///     Used to create a quest pool during each cycle of repeatable quest generation. The pool will be subsequently
    ///     narrowed down during quest generation to avoid duplicate quests. Like duplicate extractions or elimination quests
    ///     where you have to e.g. kill scavs in same locations
    /// </summary>
    /// <param name="repeatableConfig">main repeatable quest config</param>
    /// <param name="pmcLevel">Players level</param>
    /// <returns>Allowed quest pool</returns>
    protected QuestTypePool GenerateQuestPool(RepeatableQuestConfig repeatableConfig, int pmcLevel)
    {
        var questPool = CreateEmptyQuestPool(repeatableConfig);

        // Populate Exploration and Pickup quest locations
        foreach (var (location, value) in repeatableConfig.Locations)
        {
            if (location != ELocationName.any)
            {
                questPool.Pool.Exploration.Locations[location] = value;
                questPool.Pool.Pickup.Locations[location] = value;
            }
        }

        // Add "any" to pickup quest pool
        questPool.Pool.Pickup.Locations[ELocationName.any] = ["any"];

        var eliminationConfig = repeatableQuestHelper.GetEliminationConfigByPmcLevel(pmcLevel, repeatableConfig);
        var targetsConfig = new ProbabilityObjectArray<string, BossInfo>(cloner, eliminationConfig.Targets);

        // Populate Elimination quest targets and their locations
        foreach (var target in targetsConfig)
        {
            // Target is boss
            if (target.Data?.IsBoss ?? false)
            {
                questPool.Pool.Elimination.Targets.Add(target.Key, new TargetLocation { Locations = ["any"] });

                continue;
            }

            // Target is not boss
            var possibleLocations = repeatableConfig.Locations.Keys;
            var allowedLocations =
                target.Key == "Savage"
                    ? possibleLocations.Where(location => location != ELocationName.laboratory) // Exclude labs for Savage targets.
                    : possibleLocations;

            questPool.Pool.Elimination.Targets.Add(
                target.Key,
                new TargetLocation { Locations = allowedLocations.Select(x => x.ToString()).ToList() }
            );
        }

        return questPool;
    }

    /// <summary>
    ///     Create a pool of quests to generate quests from
    /// </summary>
    /// <param name="repeatableConfig">Main repeatable config</param>
    /// <returns>QuestTypePool</returns>
    protected QuestTypePool CreateEmptyQuestPool(RepeatableQuestConfig repeatableConfig)
    {
        return new QuestTypePool
        {
            Types = cloner.Clone(repeatableConfig.Types)!,
            Pool = new QuestPool
            {
                Exploration = new ExplorationPool { Locations = new Dictionary<ELocationName, List<string>>() },
                Elimination = new EliminationPool { Targets = new Dictionary<string, TargetLocation>() },
                Pickup = new ExplorationPool { Locations = new Dictionary<ELocationName, List<string>>() },
            },
        };
    }

    /// <summary>
    ///     Get count of repeatable quests profile should have access to
    /// </summary>
    /// <param name="repeatableConfig"></param>
    /// <param name="fullProfile">Full player profile</param>
    /// <returns>Quest count</returns>
    protected int GetQuestCount(RepeatableQuestConfig repeatableConfig, SptProfile fullProfile)
    {
        var questCount = repeatableConfig.NumQuests;
        if (questCount == 0)
        {
            logger.Warning($"Repeatable: {repeatableConfig.Name} quests have a count of 0");
        }

        // Add elite bonus to daily quests
        if (
            string.Equals(repeatableConfig.Name, "daily", StringComparison.OrdinalIgnoreCase)
            && profileHelper.HasEliteSkillLevel(SkillTypes.Charisma, fullProfile.CharacterData.PmcData)
        )
        // Elite charisma skill gives extra daily quest(s)
        {
            questCount += databaseService
                .GetGlobals()
                .Configuration.SkillsSettings.Charisma.BonusSettings.EliteBonusSettings.RepeatableQuestExtraCount;
        }

        // Add any extra repeatable quests the profile has unlocked
        questCount += (int)fullProfile.SptData.ExtraRepeatableQuests.GetValueOrDefault(repeatableConfig.Id, 0);

        return questCount;
    }
}
