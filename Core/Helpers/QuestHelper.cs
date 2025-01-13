using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using ILogger = Core.Models.Utils.ILogger;
using Product = Core.Models.Eft.ItemEvent.Product;

namespace Core.Helpers;

[Injectable]
public class QuestHelper
{
    private readonly ILogger _logger;
    private readonly TimeUtil _timeUtil;
    private readonly DatabaseService _databaseService;
    private readonly QuestConditionHelper _questConditionHelper;
    private readonly ProfileHelper _profileHelper;
    private readonly LocalisationService _localisationService;
    private readonly LocaleService _localeService;
    private readonly ICloner _cloner;
    private readonly QuestConfig _questConfig;

    public QuestHelper(
        ILogger logger,
        TimeUtil timeUtil,
        DatabaseService databaseService,
        QuestConditionHelper questConditionHelper,
        ProfileHelper profileHelper,
        LocalisationService localisationService,
        LocaleService localeService,
        ConfigServer configServer,
        ICloner Cloner)
    {
        _logger = logger;
        _timeUtil = timeUtil;
        _databaseService = databaseService;
        _questConditionHelper = questConditionHelper;
        _profileHelper = profileHelper;
        _localisationService = localisationService;
        _localeService = localeService;
        _cloner = Cloner;

        _questConfig = configServer.GetConfig<QuestConfig>(ConfigTypes.QUEST);
    }

    /// <summary>
    /// Get status of a quest in player profile by its id
    /// </summary>
    /// <param name="pmcData">Profile to search</param>
    /// <param name="questId">Quest id to look up</param>
    /// <returns>QuestStatus enum</returns>
    public QuestStatusEnum GetQuestStatus(PmcData pmcData, string questId)
    {
        var quest = pmcData.Quests?.FirstOrDefault((q) => q.QId == questId);

        return quest?.Status ?? QuestStatusEnum.Locked;
    }

    /// <summary>
    /// returns true if the level condition is satisfied
    /// </summary>
    /// <param name="playerLevel">Players level</param>
    /// <param name="condition">Quest condition</param>
    /// <returns>true if player level is greater than or equal to quest</returns>
    public bool DoesPlayerLevelFulfilCondition(double playerLevel, QuestCondition condition)
    {
        if (condition.ConditionType == "Level")
        {
            var conditionValue = double.Parse(condition.Value.ToString());
            switch (condition.CompareMethod)
            {
                case ">=":
                    return playerLevel >= conditionValue;
                case ">":
                    return playerLevel > conditionValue;
                case "<":
                    return playerLevel < conditionValue;
                case "<=":
                    return playerLevel <= conditionValue;
                case "=":
                    return playerLevel == conditionValue;
                default:
                    _logger.Error(
                        _localisationService.GetText("quest-unable_to_find_compare_condition", condition.CompareMethod));

                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Get the quests found in both lists (inner join)
    /// </summary>
    /// <param name="before">List of quests #1</param>
    /// <param name="after">List of quests #2</param>
    /// <returns>Reduction of cartesian product between two quest lists</returns>
    public List<Quest> GetDeltaQuests(List<Quest> before, List<Quest> after)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Adjust skill experience for low skill levels, mimicking the official client
    /// </summary>
    /// <param name="profileSkill">the skill experience is being added to</param>
    /// <param name="progressAmount">the amount of experience being added to the skill</param>
    /// <returns>the adjusted skill progress gain</returns>
    public int AdjustSkillExpForLowLevels(Common profileSkill, int progressAmount)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Get quest name by quest id
    /// </summary>
    /// <param name="questId">id to get</param>
    /// <returns></returns>
    public string GetQuestNameFromLocale(string questId)
    {
        var questNameKey = $"{ questId} name";
        return _localeService.GetLocaleDb().GetValueOrDefault(questNameKey, "UNKNOWN");
    }

    /// <summary>
    /// Check if trader has sufficient loyalty to fulfill quest requirement
    /// </summary>
    /// <param name="questProperties">Quest props</param>
    /// <param name="profile">Player profile</param>
    /// <returns>true if loyalty is high enough to fulfill quest requirement</returns>
    public bool TraderLoyaltyLevelRequirementCheck(QuestCondition questProperties, PmcData profile)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Check if trader has sufficient standing to fulfill quest requirement
    /// </summary>
    /// <param name="questProperties">Quest props</param>
    /// <param name="profile">Player profile</param>
    /// <returns>true if standing is high enough to fulfill quest requirement</returns>
    public bool TraderStandingRequirementCheck(QuestCondition questProperties, PmcData profile)
    {
        throw new System.NotImplementedException();
    }

    protected bool CompareAvailableForValues(int current, int required, string compareMethod)
    {
        switch (compareMethod)
        {
            case ">=":
                return current >= required;
            case ">":
                return current > required;
            case "<=":
                return current <= required;
            case "<":
                return current < required;
            case "!=":
                return current != required;
            case "==":
                return current == required;

            default:
                _logger.Error(_localisationService.GetText("quest-compare_operator_unhandled", compareMethod));

                return false;
        }
    }

    /**
     * Look up quest in db by accepted quest id and construct a profile-ready object ready to store in profile
     * @param pmcData Player profile
     * @param newState State the new quest should be in when returned
     * @param acceptedQuest Details of accepted quest from client
     */
    public QuestStatus GetQuestReadyForProfile(
        PmcData pmcData,
        QuestStatus newState,
        AcceptQuestRequestData acceptedQuest
    )
    {
        throw new NotImplementedException();
    }

    /**
     * Get quests that can be shown to player after starting a quest
     * @param startedQuestId Quest started by player
     * @param sessionID Session id
     * @returns Quests accessible to player including newly unlocked quests now quest (startedQuestId) was started
     */
    public List<Quest> GetNewlyAccessibleQuestsWhenStartingQuest(string startedQuestId, string sessionID)
    {
        throw new NotImplementedException();
    }

    /**
     * Should a seasonal/event quest be shown to the player
     * @param questId Quest to check
     * @returns true = show to player
     */
    public bool ShowEventQuestToPlayer(string questId)
    {
        throw new NotImplementedException();
    }

    /**
     * Is the quest for the opposite side the player is on
     * @param playerSide Player side (usec/bear)
     * @param questId QuestId to check
     */
    public bool QuestIsForOtherSide(string playerSide, string questId)
    {
        var isUsec = playerSide.ToLower() == "usec";
        if (isUsec && _questConfig.BearOnlyQuests.Contains(questId))
        {
            // Player is usec and quest is bear only, skip
            return true;
        }

        if (!isUsec && _questConfig.UsecOnlyQuests.Contains(questId))
        {
            // Player is bear and quest is usec only, skip
            return true;
        }

        return false;
    }

    /**
     * Is the provided quest prevented from being viewed by the provided game version
     * (Inclusive filter)
     * @param gameVersion Game version to check against
     * @param questId Quest id to check
     * @returns True Quest should not be visible to game version
     */
    protected bool QuestIsProfileBlacklisted(string gameVersion, string questId)
    {
        throw new NotImplementedException();
    }

    /**
     * Is the provided quest able to be seen by the provided game version
     * (Exclusive filter)
     * @param gameVersion Game version to check against
     * @param questId Quest id to check
     * @returns True Quest should be visible to game version
     */
    protected bool QuestIsProfileWhitelisted(string gameVersion, string questId)
    {
        var questBlacklist = _questConfig.ProfileBlacklist.GetValueOrDefault(gameVersion);
        if (questBlacklist is null)
        {
            // Not blacklisted
            return false;
        }

        return questBlacklist.Contains(questId);
    }

    /**
     * Get quests that can be shown to player after failing a quest
     * @param failedQuestId Id of the quest failed by player
     * @param sessionId Session id
     * @returns List of Quest
     */
    public List<Quest> FailedUnlocked(string failedQuestId, string sessionId)
    {
        throw new NotImplementedException();
    }

    /**
     * Sets the item stack to new value, or delete the item if value <= 0
     * // TODO maybe merge this function and the one from customization
     * @param pmcData Profile
     * @param itemId id of item to adjust stack size of
     * @param newStackSize Stack size to adjust to
     * @param sessionID Session id
     * @param output ItemEvent router response
     */
    public void ChangeItemStack(
        PmcData pmcData,
        string itemId,
        double newStackSize,
        string sessionID,
        ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /**
     * Add item stack change object into output route event response
     * @param output Response to add item change event into
     * @param sessionId Session id
     * @param item Item that was adjusted
     */
    protected void AddItemStackSizeChangeIntoEventResponse(
        ItemEventRouterResponse output,
        string sessionId,
        Item item)
    {
        output.ProfileChanges[sessionId].Items.ChangedItems.Add( new Product{
            Id = item.Id,
            Template = item.Template,
            ParentId = item.ParentId,
            SlotId = item.SlotId,
            Location = (ItemLocation)item.Location,
            Upd = new Upd { StackObjectsCount = item.Upd.StackObjectsCount },
        });
    }

    /**
     * Get quests, strip all requirement conditions except level
     * @param quests quests to process
     * @returns quest list without conditions
     */
    protected List<Quest> GetQuestsWithOnlyLevelRequirementStartCondition(List<Quest> quests)
    {
        return quests.Select(GetQuestWithOnlyLevelRequirementStartCondition).ToList();
    }

    /**
     * Remove all quest conditions except for level requirement
     * @param quest quest to clean
     * @returns reset Quest object
     */
    public Quest GetQuestWithOnlyLevelRequirementStartCondition(Quest quest)
    {
        var updatedQuest = _cloner.Clone(quest);
        updatedQuest.Conditions.AvailableForStart = updatedQuest.Conditions.AvailableForStart.Where(
            (q) => q.ConditionType == "Level").ToList();

        return updatedQuest;
    }

    /**
     * Fail a quest in a player profile
     * @param pmcData Player profile
     * @param failRequest Fail quest request data
     * @param sessionID Session id
     * @param output Client output
     */
    public void FailQuest(
        PmcData pmcData,
        FailQuestRequestData failRequest,
        string sessionID,
        ItemEventRouterResponse output = null)
    {
        throw new NotImplementedException();
    }

    /**
     * Get List of All Quests from db
     * NOT CLONED
     * @returns List of Quest objects
     */
    public List<Quest> GetQuestsFromDb()
    {
        return _databaseService.GetQuests().Values.ToList();
    }

    /**
     * Get quest by id from database (repeatables are stored in profile, check there if questId not found)
     * @param questId Id of quest to find
     * @param pmcData Player profile
     * @returns IQuest object
     */
    public Quest GetQuestFromDb(string questId, PmcData pmcData)
    {
        // May be a repeatable quest
        var quest = _databaseService.GetQuests()[questId];
        if (quest == null)
        {
            // Check daily/weekly objects
            foreach (var repeatableQuest in pmcData.RepeatableQuests)
            {
                quest = repeatableQuest.ActiveQuests.FirstOrDefault(r => r.Id == questId);
                if (quest != null)
                    break;
            }
        }
        
        return quest;
    }

    /// <summary>
    /// Get a quests startedMessageText key from db, if no startedMessageText key found, use description key instead
    /// </summary>
    /// <param name="startedMessageTextId">startedMessageText property from Quest</param>
    /// <param name="questDescriptionId">description property from Quest</param>
    /// <returns>message id</returns>
    public string GetMessageIdForQuestStart(string startedMessageTextId, string questDescriptionId)
    {
        // Blank or is a guid, use description instead
        var startedMessageText = GetQuestLocaleIdFromDb(startedMessageTextId);
        if (
            startedMessageText is null ||
            startedMessageText.Trim() == "" ||
                startedMessageText.ToLower() == "test" ||
                startedMessageText.Length == 24
        )
        {
            return questDescriptionId;
        }

        return startedMessageTextId;
    }

    /// <summary>
    /// Get the locale Id from locale db for a quest message
    /// </summary>
    /// <param name="questMessageId">Quest message id to look up</param>
    /// <returns>Locale Id from locale db</returns>
    public string GetQuestLocaleIdFromDb(string questMessageId)
    {
        var locale = _localeService.GetLocaleDb();
        return locale.GetValueOrDefault(questMessageId, null);
    }

    /// <summary>
    /// Alter a quests state + Add a record to its status timers object
    /// </summary>
    /// <param name="pmcData">Profile to update</param>
    /// <param name="newQuestState">New state the quest should be in</param>
    /// <param name="questId">Id of the quest to alter the status of</param>
    public void UpdateQuestState(PmcData pmcData, QuestStatusEnum newQuestState, string questId)
    {
        // Find quest in profile, update status to desired status
        var questToUpdate = pmcData.Quests.FirstOrDefault((quest) => quest.QId == questId);
        if (questToUpdate is not null)
        {
            questToUpdate.Status = newQuestState;
            questToUpdate.StatusTimers[newQuestState] = _timeUtil.GetTimeStamp();
        }
    }

    /// <summary>
    /// Resets a quests values back to its chosen state
    /// </summary>
    /// <param name="pmcData">Profile to update</param>
    /// <param name="newQuestState">New state the quest should be in</param>
    /// <param name="questId">Id of the quest to alter the status of</param>
    public void ResetQuestState(PmcData pmcData, QuestStatus newQuestState, string questId)
    {
        throw new NotImplementedException();
    }

    /**
 * Find quest with 'findItem' condition that needs the item tpl be handed in
 * @param itemTpl item tpl to look for
 * @param questIds Quests to search through for the findItem condition
 * @returns quest id with 'FindItem' condition id
 */
    public Dictionary<string, string> GetFindItemConditionByQuestItem(
        string itemTpl,
        string[] questIds,
        List<Quest> allQuests
    )
    {
        throw new NotImplementedException();
    }

    /**
     * Add all quests to a profile with the provided statuses
     * @param pmcProfile profile to update
     * @param statuses statuses quests should have added to profile
     */
    public void AddAllQuestsToProfile(PmcData pmcProfile, List<QuestStatusEnum> statuses)
    {
        // Iterate over all quests in db
        var quests = _databaseService.GetQuests();
        foreach (var (key, questData) in quests) {
            // Quest from db matches quests in profile, skip
            if (pmcProfile.Quests.Any((x) => x.QId == questData.Id))
            {
                continue;
            }

            // Create dict of status to add to quest in profile
            var statusesDict = new Dictionary<QuestStatusEnum, long>();
            foreach (var status in statuses)
            {
                statusesDict.Add(status, _timeUtil.GetTimeStamp());
            }

            var questRecordToAdd = new QuestStatus {
                QId = key,
                StartTime = _timeUtil.GetTimeStamp(),
                Status = statuses[^1], // Get last status in list as currently active status
                StatusTimers = statusesDict,
                CompletedConditions = [],
                AvailableAfter = 0,
            };

            // Check if the quest already exists in the profile
            var existingQuest = pmcProfile.Quests.FirstOrDefault(x => x.QId == key);
            if (existingQuest != null)
            {
                // Update existing quest
                existingQuest.Status = questRecordToAdd.Status;
                existingQuest.StatusTimers = questRecordToAdd.StatusTimers;
            }
            else
            {
                // Add new quest to the profile
                pmcProfile.Quests.Add(questRecordToAdd);
            }
        }
    }

    public void FindAndRemoveQuestFromArrayIfExists(string questId, List<QuestStatus> quests)
    {
        var pmcQuestToReplaceStatus = quests.FirstOrDefault((quest) => quest.QId == questId);
        if (pmcQuestToReplaceStatus is not null)
        {
            var index = quests.IndexOf(pmcQuestToReplaceStatus);
            quests.RemoveAt(index);
        }
    }

    /**
     * Return a list of quests that would fail when supplied quest is completed
     * @param completedQuestId quest completed id
     * @returns array of Quest objects
     */
    public List<Quest> GetQuestsFailedByCompletingQuest(string completedQuestId)
    {
        throw new NotImplementedException();
    }

    /**
     * Get the hours a mails items can be collected for by profile type
     * @param pmcData Profile to get hours for
     * @returns Hours item will be available for
     */
    public int GetMailItemRedeemTimeHoursForProfile(PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse CompleteQuest(
        PmcData pmcData,
        CompleteQuestRequestData body,
        string sessionID
    )
    {
        throw new NotImplementedException();
    }

    /**
     * Handle client/quest/list
     * Get all quests visible to player
     * Exclude quests with incomplete preconditions (level/loyalty)
     * @param sessionID session id
     * @returns array of Quest
     */
    public List<Quest> GetClientQuests(string sessionID)
    {
        List<Quest> questsToShowPlayer = new List<Quest>();
        var profile = _profileHelper.GetPmcProfile(sessionID);

        var allQuests = GetQuestsFromDb();
        foreach (var quest in allQuests)
        {
            // Player already accepted the quest, show it regardless of status
            var questInProfile = profile.Quests.FirstOrDefault(x => x.QId == quest.Id);
            if (questInProfile is not null)
            {
                quest.SptStatus = questInProfile.Status;
                questsToShowPlayer.Add(quest);
                continue;
            }

            // Filter out bear quests for usec and vice versa
            if (QuestIsForOtherSide(profile.Info.Side, quest.Id))
            {
                continue;
            }

            if (!ShowEventQuestToPlayer(quest.Id))
            {
                continue;
            }

            // Don't add quests that have a level higher than the user's
            if (!PlayerLevelFulfillsQuestRequirement(quest, profile.Info.Level.Value))
            {
                continue;
            }

            // Player can use trader mods then remove them, leaving quests behind
            if (!profile.TradersInfo.TryGetValue(quest.TraderId, out var trader))
            {
                _logger.Debug($"Unable to show quest: {quest.QuestName} as its for a trader: {quest.TraderId} that no longer exists.");
                continue;
            }

            var questRequirements = _questConditionHelper.GetQuestConditions(quest.Conditions.AvailableForStart);
            var loyaltyRequirements = _questConditionHelper.GetLoyaltyConditions(quest.Conditions.AvailableForStart);
            var standingRequirements = _questConditionHelper.GetStandingConditions(quest.Conditions.AvailableForStart);

            // Quest has no conditions, standing or loyalty conditions, add to visible quest list
            if (questRequirements.Count == 0 && loyaltyRequirements.Count == 0 && standingRequirements.Count == 0)
            {
                quest.SptStatus = QuestStatusEnum.AvailableForStart;
                questsToShowPlayer.Add(quest);
                continue;
            }

            // Check the status of each quest condition, if any are not completed
            // then this quest should not be visible
            bool haveCompletedPreviousQuest = true;
            foreach (var conditionToFulfil in questRequirements)
            {
                // If the previous quest isn't in the user profile, it hasn't been completed or started
                var prerequisiteQuest = profile.Quests.FirstOrDefault(profileQuest => (conditionToFulfil.Target as string[]).Contains(profileQuest.QId));

                if (prerequisiteQuest is null)
                {
                    haveCompletedPreviousQuest = false;
                    break;
                }

                // Prereq does not have its status requirement fulfilled
                // Some bsg status ids are strings, MUST convert to number before doing includes check
                if (!conditionToFulfil.Status.Contains(prerequisiteQuest.Status.Value))
                {
                    haveCompletedPreviousQuest = false;
                    break;
                }

                // Has a wait timer
                if (conditionToFulfil.AvailableAfter > 0)
                {
                    // Compare current time to unlock time for previous quest
                    prerequisiteQuest.StatusTimers.TryGetValue(prerequisiteQuest.Status.Value, out var previousQuestCompleteTime);
                    var unlockTime = previousQuestCompleteTime + conditionToFulfil.AvailableAfter;
                    if (unlockTime > _timeUtil.GetTimeStamp())
                    {
                        _logger.Debug(
                            $"Quest { quest.QuestName} is locked for another: {unlockTime - _timeUtil.GetTimeStamp()} seconds");
                    }
                }
            }

            // Previous quest not completed, skip
            if (!haveCompletedPreviousQuest)
            {
                continue;
            }

            var passesLoyaltyRequirements = true;
            foreach (var condition in loyaltyRequirements) {
                if (!TraderLoyaltyLevelRequirementCheck(condition, profile))
                {
                    passesLoyaltyRequirements = false;
                    break;
                }
            }

            var passesStandingRequirements = true;
            foreach (var condition in standingRequirements) {
                if (!TraderStandingRequirementCheck(condition, profile))
                {
                    passesStandingRequirements = false;
                    break;
                }
            }

            if (haveCompletedPreviousQuest && passesLoyaltyRequirements && passesStandingRequirements)
            {
                quest.SptStatus = QuestStatusEnum.AvailableForStart;
                questsToShowPlayer.Add(quest);
            }
        }

        return questsToShowPlayer;
    }


    /**
     * Create a clone of the given quest array with the rewards updated to reflect the
     * given game version
     * @param quests List of quests to check
     * @param gameVersion Game version of the profile
     * @returns Array of Quest objects with the rewards filtered correctly for the game version
     */
    protected List<Quest> UpdateQuestsForGameEdition(List<Quest> quests, string gameVersion)
    {
        throw new NotImplementedException();
    }

    /**
     * Return a list of quests that would fail when supplied quest is completed
     * @param completedQuestId Quest completed id
     * @returns Array of Quest objects
     */
    protected List<Quest> GetQuestsFromProfileFailedByCompletingQuest(string completedQuestId, PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /**
     * Fail the provided quests
     * Update quest in profile, otherwise add fresh quest object with failed status
     * @param sessionID session id
     * @param pmcData player profile
     * @param questsToFail quests to fail
     * @param output Client output
     */
    protected void FailQuests(
        string sessionID,
        PmcData pmcData,
        List<Quest> questsToFail,
        ItemEventRouterResponse output
    )
    {
        throw new NotImplementedException();
    }

    /**
     * Send a popup to player on successful completion of a quest
     * @param sessionID session id
     * @param pmcData Player profile
     * @param completedQuestId Completed quest id
     * @param questRewards Rewards given to player
     */
    protected void SendSuccessDialogMessageOnQuestComplete(
        string sessionID,
        PmcData pmcData,
        string completedQuestId,
        List<Item> questRewards
    )
    {
        throw new NotImplementedException();
    }

    /**
     * Look for newly available quests after completing a quest with a requirement to wait x minutes (time-locked) before being available and add data to profile
     * @param pmcData Player profile to update
     * @param quests Quests to look for wait conditions in
     * @param completedQuestId Quest just completed
     */
    protected void AddTimeLockedQuestsToProfile(PmcData pmcData, List<Quest> quests, string completedQuestId)
    {
        throw new NotImplementedException();
    }

    /**
     * Remove a quest entirely from a profile
     * @param sessionId Player id
     * @param questIdToRemove Qid of quest to remove
     */
    protected void RemoveQuestFromScavProfile(string sessionId, string questIdToRemove)
    {
        throw new NotImplementedException();
    }

    /**
     * Return quests that have different statuses
     * @param preQuestStatusus Quests before
     * @param postQuestStatuses Quests after
     * @returns QuestStatusChange array
     */
    protected List<QuestStatus> GetQuestsWithDifferentStatuses(
        List<QuestStatus> preQuestStatusus,
        List<QuestStatus> postQuestStatuses
    )
    {
        throw new NotImplementedException();
    }

    /**
     * Does a provided quest have a level requirement equal to or below defined level
     * @param quest Quest to check
     * @param playerLevel level of player to test against quest
     * @returns true if quest can be seen/accepted by player of defined level
     */
    protected bool PlayerLevelFulfillsQuestRequirement(Quest quest, double playerLevel)
    {
        if (quest.Conditions is null)
        {
            // No conditions
            return true;
        }

        var levelConditions = _questConditionHelper.GetLevelConditions(quest.Conditions.AvailableForStart);
        if (levelConditions is not null)
        {
            foreach (var levelCondition in levelConditions)
            {
                if (!DoesPlayerLevelFulfilCondition(playerLevel, levelCondition))
                {
                    // Not valid, exit out
                    return false;
                }
            }
        }

        // All conditions passed / has no level requirement, valid
        return true;
    }
}
