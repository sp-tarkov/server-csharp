using System.Globalization;
using SPTarkov.Common.Annotations;
using SPTarkov.Common.Extensions;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class QuestHelper(
    ISptLogger<QuestHelper> _logger,
    TimeUtil _timeUtil,
    HashUtil _hashUtil,
    ItemHelper _itemHelper,
    DatabaseService _databaseService,
    QuestConditionHelper _questConditionHelper,
    EventOutputHolder _eventOutputHolder,
    LocaleService _localeService,
    ProfileHelper _profileHelper,
    QuestRewardHelper _questRewardHelper,
    RewardHelper _rewardHelper,
    LocalisationService _localisationService,
    SeasonalEventService _seasonalEventService,
    TraderHelper _traderHelper,
    MailSendService _mailSendService,
    PlayerService _playerService,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected HashSet<QuestStatusEnum> _newlyQuestCheck =
    [
        QuestStatusEnum.Started,
        QuestStatusEnum.AvailableForFinish,
    ];
    protected QuestConfig _questConfig = _configServer.GetConfig<QuestConfig>();

    /// <summary>
    ///     Get status of a quest in player profile by its id
    /// </summary>
    /// <param name="pmcData">Profile to search</param>
    /// <param name="questId">Quest id to look up</param>
    /// <returns>QuestStatus enum</returns>
    public QuestStatusEnum GetQuestStatus(PmcData pmcData, string questId)
    {
        var quest = pmcData.Quests?.FirstOrDefault(q =>
        {
            return q.QId == questId;
        });

        return quest?.Status ?? QuestStatusEnum.Locked;
    }

    /// <summary>
    ///     returns true if the level condition is satisfied
    /// </summary>
    /// <param name="playerLevel">Players level</param>
    /// <param name="condition">Quest condition</param>
    /// <returns>true if player level is greater than or equal to quest</returns>
    public bool DoesPlayerLevelFulfilCondition(double playerLevel, QuestCondition condition)
    {
        if (condition.ConditionType != "Level")
        {
            return true;
        }

        var conditionValue = double.Parse(condition.Value.ToString(), CultureInfo.InvariantCulture);
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
                    _localisationService.GetText(
                        "quest-unable_to_find_compare_condition",
                        condition.CompareMethod
                    )
                );

                return false;
        }
    }

    /// <summary>
    ///     Get the quests found in both lists (inner join)
    /// </summary>
    /// <param name="before">List of quests #1</param>
    /// <param name="after">List of quests #2</param>
    /// <returns>Reduction of cartesian product between two quest lists</returns>
    public List<Quest> GetDeltaQuests(List<Quest> before, List<Quest> after)
    {
        List<string> knownQuestsIds = [];
        foreach (var quest in before)
        {
            knownQuestsIds.Add(quest.Id);
        }

        if (knownQuestsIds.Count != 0)
        {
            return after
                .Where(q =>
                {
                    return knownQuestsIds.IndexOf(q.Id) == -1;
                })
                .ToList();
        }

        return after;
    }

    /// <summary>
    ///     Adjust skill experience for low skill levels, mimicking the official client
    /// </summary>
    /// <param name="profileSkill">the skill experience is being added to</param>
    /// <param name="progressAmount">the amount of experience being added to the skill</param>
    /// <returns>the adjusted skill progress gain</returns>
    public int AdjustSkillExpForLowLevels(
        Models.Eft.Common.Tables.Common profileSkill,
        int progressAmount
    )
    {
        var currentLevel = Math.Floor((double)(profileSkill.Progress / 100));

        // Only run this if the current level is under 9
        if (currentLevel >= 9)
        {
            return progressAmount;
        }

        // This calculates how much progress we have in the skill's starting level
        var startingLevelProgress = profileSkill.Progress % 100 * ((currentLevel + 1) / 10);

        // The code below assumes a 1/10th progress skill amount
        var remainingProgress = progressAmount / 10;

        // We have to do this loop to handle edge cases where the provided XP bumps your level up
        // See "CalculateExpOnFirstLevels" in client for original logic
        var adjustedSkillProgress = 0;
        while (remainingProgress > 0 && currentLevel < 9)
        {
            // Calculate how much progress to add, limiting it to the current level max progress
            var currentLevelRemainingProgress = (currentLevel + 1) * 10 - startingLevelProgress;
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"currentLevelRemainingProgress: {currentLevelRemainingProgress}");
            }

            var progressToAdd = Math.Min(remainingProgress, currentLevelRemainingProgress ?? 0);
            var adjustedProgressToAdd = 10 / (currentLevel + 1) * progressToAdd;
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug(
                    $"Progress To Add: {progressToAdd}  Adjusted for level: {adjustedProgressToAdd}"
                );
            }

            // Add the progress amount adjusted by level
            adjustedSkillProgress += (int)adjustedProgressToAdd;
            remainingProgress -= (int)progressToAdd;
            startingLevelProgress = 0;
            currentLevel++;
        }

        // If there's any remaining progress, add it. This handles if you go from level 8 -> 9
        if (remainingProgress > 0)
        {
            adjustedSkillProgress += remainingProgress;
        }

        return adjustedSkillProgress;
    }

    /// <summary>
    ///     Get quest name by quest id
    /// </summary>
    /// <param name="questId">id to get</param>
    /// <returns></returns>
    public string GetQuestNameFromLocale(string questId)
    {
        var questNameKey = $"{questId} name";
        return _localeService.GetLocaleDb().GetValueOrDefault(questNameKey, "UNKNOWN");
    }

    /// <summary>
    ///     Check if trader has sufficient loyalty to fulfill quest requirement
    /// </summary>
    /// <param name="questProperties">Quest props</param>
    /// <param name="profile">Player profile</param>
    /// <returns>true if loyalty is high enough to fulfill quest requirement</returns>
    public bool TraderLoyaltyLevelRequirementCheck(QuestCondition questProperties, PmcData profile)
    {
        if (
            !profile.TradersInfo.TryGetValue(
                questProperties.Target.IsItem
                    ? questProperties.Target.Item
                    : questProperties.Target.List.FirstOrDefault(),
                out var trader
            )
        )
        {
            _logger.Error(
                _localisationService.GetText(
                    "quest-unable_to_find_trader_in_profile",
                    questProperties.Target
                )
            );
        }

        return CompareAvailableForValues(
            trader.LoyaltyLevel.Value,
            questProperties.Value.Value,
            questProperties.CompareMethod
        );
    }

    /// <summary>
    ///     Check if trader has sufficient standing to fulfill quest requirement
    /// </summary>
    /// <param name="questProperties">Quest props</param>
    /// <param name="profile">Player profile</param>
    /// <returns>true if standing is high enough to fulfill quest requirement</returns>
    public bool TraderStandingRequirementCheck(QuestCondition questProperties, PmcData profile)
    {
        var requiredLoyaltyLevel = int.Parse(questProperties.Value.ToString());
        if (
            !profile.TradersInfo.TryGetValue(
                questProperties.Target.IsItem
                    ? questProperties.Target.Item
                    : questProperties.Target.List.FirstOrDefault(),
                out var trader
            )
        )
        {
            _logger.Error(
                _localisationService.GetText(
                    "quest-unable_to_find_trader_in_profile",
                    questProperties.Target
                )
            );
        }

        return CompareAvailableForValues(
            trader.Standing ?? 1,
            requiredLoyaltyLevel,
            questProperties.CompareMethod
        );
    }

    protected bool CompareAvailableForValues(double current, double required, string compareMethod)
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
                _logger.Error(
                    _localisationService.GetText("quest-compare_operator_unhandled", compareMethod)
                );

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
        QuestStatusEnum newState,
        AcceptQuestRequestData acceptedQuest
    )
    {
        var currentTimestamp = _timeUtil.GetTimeStamp();
        var existingQuest = pmcData.Quests.FirstOrDefault(q =>
        {
            return q.QId == acceptedQuest.QuestId;
        });
        if (existingQuest is not null)
        {
            // Quest exists, update its status
            existingQuest.StartTime = currentTimestamp;
            existingQuest.Status = newState;
            existingQuest.StatusTimers[newState] = currentTimestamp;
            existingQuest.CompletedConditions = [];

            if (existingQuest.AvailableAfter is not null)
            {
                existingQuest.AvailableAfter = null;
            }

            return existingQuest;
        }

        // Quest doesn't exists, add it
        var newQuest = new QuestStatus
        {
            QId = acceptedQuest.QuestId,
            StartTime = currentTimestamp,
            Status = newState,
            StatusTimers = new Dictionary<QuestStatusEnum, double>(),
        };

        // Check if quest has a prereq to be placed in a 'pending' state, otherwise set status timers value
        var questDbData = GetQuestFromDb(acceptedQuest.QuestId, pmcData);
        if (questDbData is null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "quest-unable_to_find_quest_in_db",
                    new { questId = acceptedQuest.QuestId, questType = acceptedQuest.Type }
                )
            );
        }

        var waitTime = questDbData?.Conditions.AvailableForStart.FirstOrDefault(x =>
        {
            return x.AvailableAfter > 0;
        });
        if (waitTime is not null && acceptedQuest.Type != "repeatable")
        {
            // Quest should be put into 'pending' state
            newQuest.StartTime = 0;
            newQuest.Status = QuestStatusEnum.AvailableAfter; // 9
            newQuest.AvailableAfter = currentTimestamp + waitTime.AvailableAfter;
        }
        else
        {
            newQuest.StatusTimers[newState] = currentTimestamp;
            newQuest.CompletedConditions = [];
        }

        return newQuest;
    }

    /**
     * Get quests that can be shown to player after starting a quest
     * @param startedQuestId Quest started by player
     * @param sessionID Session id
     * @returns Quests accessible to player including newly unlocked quests now quest (startedQuestId) was started
     */
    public List<Quest> GetNewlyAccessibleQuestsWhenStartingQuest(
        string startedQuestId,
        string sessionID
    )
    {
        // Get quest acceptance data from profile
        var profile = _profileHelper.GetPmcProfile(sessionID);
        var startedQuestInProfile = profile.Quests.FirstOrDefault(profileQuest =>
        {
            return profileQuest.QId == startedQuestId;
        });

        // Get quests that
        var eligibleQuests = GetQuestsFromDb()
            .Where(quest =>
            {
                // Quest is accessible to player when the accepted quest passed into param is started
                // e.g. Quest A passed in, quest B is looped over and has requirement of A to be started, include it
                var acceptedQuestCondition = quest.Conditions.AvailableForStart.FirstOrDefault(
                    condition =>
                    {
                        return condition.ConditionType == "Quest"
                            && (
                                (condition.Target?.Item?.Contains(startedQuestId) ?? false)
                                || (condition.Target?.List?.Contains(startedQuestId) ?? false)
                            )
                            && (condition.Status?.Contains(QuestStatusEnum.Started) ?? false);
                    }
                );

                // Not found, skip quest
                if (acceptedQuestCondition is null)
                {
                    return false;
                }

                // Skip locked event quests
                if (!ShowEventQuestToPlayer(quest.Id))
                {
                    return false;
                }

                // Skip quest if its flagged as for other side
                if (QuestIsForOtherSide(profile.Info.Side, quest.Id))
                {
                    return false;
                }

                if (QuestIsProfileBlacklisted(profile.Info.GameVersion, quest.Id))
                {
                    return false;
                }

                if (QuestIsProfileWhitelisted(profile.Info.GameVersion, quest.Id))
                {
                    return false;
                }

                var standingRequirements = _questConditionHelper.GetStandingConditions(
                    quest.Conditions.AvailableForStart
                );
                foreach (var condition in standingRequirements)
                {
                    if (!TraderStandingRequirementCheck(condition, profile))
                    {
                        return false;
                    }
                }

                var loyaltyRequirements = _questConditionHelper.GetLoyaltyConditions(
                    quest.Conditions.AvailableForStart
                );
                foreach (var condition in loyaltyRequirements)
                {
                    if (!TraderLoyaltyLevelRequirementCheck(condition, profile))
                    {
                        return false;
                    }
                }

                // Include if quest found in profile and is started or ready to hand in
                return startedQuestInProfile is not null
                    && _newlyQuestCheck.Contains((QuestStatusEnum)startedQuestInProfile.Status);
            });

        return GetQuestsWithOnlyLevelRequirementStartCondition(eligibleQuests);
    }

    /**
     * Should a seasonal/event quest be shown to the player
     * @param questId Quest to check
     * @returns true = show to player
     */
    public bool ShowEventQuestToPlayer(string questId)
    {
        var isChristmasEventActive = _seasonalEventService.ChristmasEventEnabled();
        var isHalloweenEventActive = _seasonalEventService.HalloweenEventEnabled();

        // Not christmas + quest is for christmas
        if (
            !isChristmasEventActive
            && _seasonalEventService.IsQuestRelatedToEvent(questId, SeasonalEventType.Christmas)
        )
        {
            return false;
        }

        // Not halloween + quest is for halloween
        if (
            !isHalloweenEventActive
            && _seasonalEventService.IsQuestRelatedToEvent(questId, SeasonalEventType.Halloween)
        )
        {
            return false;
        }

        // Should non-season event quests be shown to player
        if (
            !(_questConfig.ShowNonSeasonalEventQuests ?? false)
            && _seasonalEventService.IsQuestRelatedToEvent(questId, SeasonalEventType.None)
        )
        {
            return false;
        }

        return true;
    }

    /**
     * Is the quest for the opposite side the player is on
     * @param playerSide Player side (usec/bear)
     * @param questId QuestId to check
     */
    public bool QuestIsForOtherSide(string playerSide, string questId)
    {
        var isUsec = string.Equals(playerSide, "usec", StringComparison.OrdinalIgnoreCase);
        if (isUsec && _questConfig.BearOnlyQuests.Contains(questId))
        // Player is usec and quest is bear only, skip
        {
            return true;
        }

        if (!isUsec && _questConfig.UsecOnlyQuests.Contains(questId))
        // Player is bear and quest is usec only, skip
        {
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
        var questBlacklist = _questConfig.ProfileBlacklist?.GetValueOrDefault(gameVersion);
        if (questBlacklist is null)
        // Not blacklisted
        {
            return false;
        }

        return questBlacklist.Contains(questId);
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
        // Not blacklisted
        {
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
        var profile = _profileHelper.GetPmcProfile(sessionId);
        var profileQuest = profile.Quests.FirstOrDefault(x =>
        {
            return x.QId == failedQuestId;
        });

        var quests = GetQuestsFromDb()
            .Where(q =>
            {
                var acceptedQuestCondition = q.Conditions.AvailableForStart.FirstOrDefault(c =>
                {
                    return c.ConditionType == "Quest"
                        && (c.Target.IsList ? c.Target.List : [c.Target.Item]).Contains(
                            failedQuestId
                        )
                        && c.Status[0] == QuestStatusEnum.Fail;
                });

                if (acceptedQuestCondition is null)
                {
                    return false;
                }

                return profileQuest is not null && profileQuest.Status == QuestStatusEnum.Fail;
            })
            .ToList();

        if (quests.Any())
        {
            return quests;
        }

        return GetQuestsWithOnlyLevelRequirementStartCondition(quests);
    }

    /**
     * Sets the item stack to new value, or delete the item if value
     * <
     * =
     * 0
     * /
     * /
     * TODO
     * maybe
     * merge
     * this
     * function
     * and
     * the
     * one
     * from
     * customization
     * @
     * param
     * pmcData
     * Profile
     * @
     * param
     * itemId
     * id
     * of
     * item
     * to
     * adjust
     * stack
     * size
     * of
     * @
     * param
     * newStackSize
     * Stack
     * size
     * to
     * adjust
     * to
     * @
     * param
     * sessionID
     * Session
     * id
     * @
     * param
     * output
     * ItemEvent
     * router
     * response
     */
    public void ChangeItemStack(
        PmcData pmcData,
        string itemId,
        int newStackSize,
        string sessionID,
        ItemEventRouterResponse output
    )
    {
        var inventoryItemIndex = pmcData.Inventory.Items.FindIndex(item =>
        {
            return item.Id == itemId;
        });
        if (inventoryItemIndex < 0)
        {
            _logger.Error(
                _localisationService.GetText("quest-item_not_found_in_inventory", itemId)
            );

            return;
        }

        if (newStackSize > 0)
        {
            var item = pmcData.Inventory.Items[inventoryItemIndex];
            _itemHelper.AddUpdObjectToItem(item);

            item.Upd.StackObjectsCount = newStackSize;

            AddItemStackSizeChangeIntoEventResponse(output, sessionID, item);
        }
        else
        {
            // this case is probably dead Code right now, since the only calling function
            // checks explicitly for Value > 0.
            output.ProfileChanges[sessionID].Items.DeletedItems.Add(new Item { Id = itemId });
            pmcData.Inventory.Items.RemoveAt(inventoryItemIndex);
        }
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
        Item item
    )
    {
        output
            .ProfileChanges[sessionId]
            .Items.ChangedItems.Add(
                new Item
                {
                    Id = item.Id,
                    Template = item.Template,
                    ParentId = item.ParentId,
                    SlotId = item.SlotId,
                    Location = item.Location,
                    Upd = new Upd { StackObjectsCount = item.Upd.StackObjectsCount },
                }
            );
    }

    /**
     * Get quests, strip all requirement conditions except level
     * @param quests quests to process
     * @returns quest list without conditions
     */
    protected List<Quest> GetQuestsWithOnlyLevelRequirementStartCondition(IEnumerable<Quest> quests)
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
        updatedQuest.Conditions.AvailableForStart = updatedQuest
            .Conditions.AvailableForStart.Where(q =>
            {
                return q.ConditionType == "Level";
            })
            .ToList();

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
        ItemEventRouterResponse output = null
    )
    {
        var updatedOutput = output;

        // Prepare response to send back to client
        if (updatedOutput is null)
        {
            updatedOutput = _eventOutputHolder.GetOutput(sessionID);
        }

        UpdateQuestState(pmcData, QuestStatusEnum.Fail, failRequest.QuestId);
        var questRewards = _questRewardHelper.ApplyQuestReward(
            pmcData,
            failRequest.QuestId,
            QuestStatusEnum.Fail,
            sessionID,
            updatedOutput
        );

        // Create a dialog message for completing the quest.
        var quest = GetQuestFromDb(failRequest.QuestId, pmcData);

        // Merge all daily/weekly/scav daily quests into one array and look for the matching quest by id
        var matchingRepeatableQuest = pmcData
            .RepeatableQuests.SelectMany(repeatableType =>
            {
                return repeatableType.ActiveQuests;
            })
            .FirstOrDefault(activeQuest =>
            {
                return activeQuest.Id == failRequest.QuestId;
            });

        // Quest found and no repeatable found
        if (quest is not null && matchingRepeatableQuest is null)
        {
            if (quest.FailMessageText.Trim().Any())
            {
                _mailSendService.SendLocalisedNpcMessageToPlayer(
                    sessionID,
                    quest?.TraderId ?? matchingRepeatableQuest?.TraderId,
                    MessageType.QUEST_FAIL,
                    quest.FailMessageText,
                    questRewards.ToList(),
                    _timeUtil.GetHoursAsSeconds((int)GetMailItemRedeemTimeHoursForProfile(pmcData))
                );
            }
        }

        updatedOutput
            .ProfileChanges[sessionID]
            .Quests.AddRange(FailedUnlocked(failRequest.QuestId, sessionID));
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
        // Maybe a repeatable quest?
        if (_databaseService.GetQuests().TryGetValue(questId, out var quest))
        {
            return quest;
        }

        // Check daily/weekly objects
        return pmcData
            .RepeatableQuests.SelectMany(x =>
            {
                return x.ActiveQuests;
            })
            .FirstOrDefault(x =>
            {
                return x.Id == questId;
            });
    }

    /// <summary>
    ///     Get a quests startedMessageText key from db, if no startedMessageText key found, use description key instead
    /// </summary>
    /// <param name="startedMessageTextId">startedMessageText property from Quest</param>
    /// <param name="questDescriptionId">description property from Quest</param>
    /// <returns>message id</returns>
    public string GetMessageIdForQuestStart(string startedMessageTextId, string questDescriptionId)
    {
        // Blank or is a guid, use description instead
        var startedMessageText = GetQuestLocaleIdFromDb(startedMessageTextId);
        if (
            startedMessageText is null
            || startedMessageText.Trim() == ""
            || string.Equals(startedMessageText, "test", StringComparison.OrdinalIgnoreCase)
            || startedMessageText.Length == 24
        )
        {
            return questDescriptionId;
        }

        return startedMessageTextId;
    }

    /// <summary>
    ///     Get the locale Id from locale db for a quest message
    /// </summary>
    /// <param name="questMessageId">Quest message id to look up</param>
    /// <returns>Locale Id from locale db</returns>
    public string GetQuestLocaleIdFromDb(string questMessageId)
    {
        var locale = _localeService.GetLocaleDb();
        return locale.GetValueOrDefault(questMessageId, null);
    }

    /// <summary>
    ///     Alter a quests state + Add a record to its status timers object
    /// </summary>
    /// <param name="pmcData">Profile to update</param>
    /// <param name="newQuestState">New state the quest should be in</param>
    /// <param name="questId">Id of the quest to alter the status of</param>
    public void UpdateQuestState(PmcData pmcData, QuestStatusEnum newQuestState, string questId)
    {
        // Find quest in profile, update status to desired status
        var questToUpdate = pmcData.Quests.FirstOrDefault(quest =>
        {
            return quest.QId == questId;
        });
        if (questToUpdate is not null)
        {
            questToUpdate.Status = newQuestState;
            questToUpdate.StatusTimers[newQuestState] = _timeUtil.GetTimeStamp();
        }
    }

    /// <summary>
    ///     Resets a quests values back to its chosen state
    /// </summary>
    /// <param name="pmcData">Profile to update</param>
    /// <param name="newQuestState">New state the quest should be in</param>
    /// <param name="questId">Id of the quest to alter the status of</param>
    public void ResetQuestState(PmcData pmcData, QuestStatusEnum newQuestState, string questId)
    {
        var questToUpdate = pmcData.Quests.FirstOrDefault(quest =>
        {
            return quest.QId == questId;
        });
        if (questToUpdate is not null)
        {
            var currentTimestamp = _timeUtil.GetTimeStamp();

            questToUpdate.Status = newQuestState;

            // Only set start time when quest is being started
            if (newQuestState == QuestStatusEnum.Started)
            {
                questToUpdate.StartTime = currentTimestamp;
            }

            questToUpdate.StatusTimers[newQuestState] = currentTimestamp;

            // Delete all status timers after applying new status
            foreach (var statusKey in questToUpdate.StatusTimers)
            {
                if (statusKey.Key > newQuestState)
                {
                    questToUpdate.StatusTimers.Remove(statusKey.Key);
                }
            }

            // Remove all completed conditions
            questToUpdate.CompletedConditions = [];
        }
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
        Dictionary<string, string> result = new();
        foreach (var questId in questIds)
        {
            var questInDb = allQuests.FirstOrDefault(x =>
            {
                return x.Id == questId;
            });
            if (questInDb is null)
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"Unable to find quest: {questId} in db, cannot get 'FindItem' condition, skipping"
                    );
                }

                continue;
            }

            var condition = questInDb.Conditions.AvailableForFinish.FirstOrDefault(c =>
            {
                return c.ConditionType == "FindItem"
                    && (
                        (c.Target.IsList ? c.Target.List : [c.Target.Item])?.Contains(itemTpl)
                        ?? false
                    );
            });
            if (condition is not null)
            {
                result[questId] = condition.Id;

                break;
            }
        }

        return result;
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
        foreach (var (key, questData) in quests)
        {
            // Quest from db matches quests in profile, skip
            if (
                pmcProfile.Quests.Any(x =>
                {
                    return x.QId == questData.Id;
                })
            )
            {
                continue;
            }

            // Create dict of status to add to quest in profile
            var statusesDict = new Dictionary<QuestStatusEnum, double>();
            foreach (var status in statuses)
            {
                statusesDict.Add(status, _timeUtil.GetTimeStamp());
            }

            var questRecordToAdd = new QuestStatus
            {
                QId = key,
                StartTime = _timeUtil.GetTimeStamp(),
                Status = statuses[^1], // Get last status in list as currently active status
                StatusTimers = statusesDict,
                CompletedConditions = [],
                AvailableAfter = 0,
            };

            // Check if the quest already exists in the profile
            var existingQuest = pmcProfile.Quests.FirstOrDefault(x =>
            {
                return x.QId == key;
            });
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
        var pmcQuestToReplaceStatus = quests.FirstOrDefault(quest =>
        {
            return quest.QId == questId;
        });
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
        var questsInDb = GetQuestsFromDb();
        return questsInDb
            .Where(quest =>
            {
                // No fail conditions, exit early
                if (quest.Conditions.Fail is null || quest.Conditions.Fail.Count == 0)
                {
                    return false;
                }

                return quest.Conditions.Fail.Any(condition =>
                {
                    return (
                            condition.Target.IsList
                                ? condition.Target.List
                                : [condition.Target.Item]
                        )?.Contains(completedQuestId) ?? false;
                });
            })
            .ToList();
    }

    /**
     * Get the hours a mails items can be collected for by profile type
     * @param pmcData Profile to get hours for
     * @returns Hours item will be available for
     */
    public double GetMailItemRedeemTimeHoursForProfile(PmcData pmcData)
    {
        if (!_questConfig.MailRedeemTimeHours.TryGetValue(pmcData.Info.GameVersion, out var value))
        {
            return _questConfig.MailRedeemTimeHours["default"] ?? 48;
        }

        return value ?? 48;
    }

    public ItemEventRouterResponse CompleteQuest(
        PmcData pmcData,
        CompleteQuestRequestData body,
        string sessionID
    )
    {
        var completeQuestResponse = _eventOutputHolder.GetOutput(sessionID);

        var preCompleteProfileQuests = _cloner.Clone(pmcData.Quests);

        var completedQuestId = body.QuestId;
        var clientQuestsClone = _cloner.Clone(GetClientQuests(sessionID)); // Must be gathered prior to applyQuestReward() & failQuests()

        const QuestStatusEnum newQuestState = QuestStatusEnum.Success;
        UpdateQuestState(pmcData, newQuestState, completedQuestId);
        var questRewards = _questRewardHelper.ApplyQuestReward(
            pmcData,
            body.QuestId,
            newQuestState,
            sessionID,
            completeQuestResponse
        );

        // Check for linked failed + unrestartable quests (only get quests not already failed
        var questsToFail = GetQuestsFromProfileFailedByCompletingQuest(completedQuestId, pmcData);
        if (questsToFail?.Count > 0)
        {
            FailQuests(sessionID, pmcData, questsToFail, completeQuestResponse);
        }

        // Show modal on player screen
        SendSuccessDialogMessageOnQuestComplete(
            sessionID,
            pmcData,
            completedQuestId,
            questRewards.ToList()
        );

        // Add diff of quests before completion vs after for client response
        var questDelta = GetDeltaQuests(clientQuestsClone, GetClientQuests(sessionID));

        // Check newly available + failed quests for timegates and add them to profile
        AddTimeLockedQuestsToProfile(pmcData, questDelta, body.QuestId);

        // Inform client of quest changes
        completeQuestResponse.ProfileChanges[sessionID].Quests.AddRange(questDelta);

        // Check if it's a repeatable quest. If so, remove from Quests
        foreach (var currentRepeatable in pmcData.RepeatableQuests)
        {
            var repeatableQuest = currentRepeatable.ActiveQuests.FirstOrDefault(activeRepeatable =>
            {
                return activeRepeatable.Id == completedQuestId;
            });
            if (repeatableQuest is not null)
            // Need to remove redundant scav quest object as its no longer necessary, is tracked in pmc profile
            {
                if (repeatableQuest.Side == "Scav")
                {
                    RemoveQuestFromScavProfile(sessionID, repeatableQuest.Id);
                }
            }
        }

        // Hydrate client response questsStatus array with data
        var questStatusChanges = GetQuestsWithDifferentStatuses(
            preCompleteProfileQuests,
            pmcData.Quests
        );
        if (questStatusChanges is not null)
        {
            completeQuestResponse
                .ProfileChanges[sessionID]
                .QuestsStatus.AddRange(questStatusChanges);
        }

        return completeQuestResponse;
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
        List<Quest> questsToShowPlayer = [];
        var profile = _profileHelper.GetPmcProfile(sessionID);
        if (profile is null)
        {
            _logger.Error($"Profile {sessionID} not found, unable to return quests");

            return [];
        }

        var allQuests = GetQuestsFromDb();
        foreach (var quest in allQuests)
        {
            // Player already accepted the quest, show it regardless of status
            var questInProfile = profile.Quests.FirstOrDefault(x =>
            {
                return x.QId == quest.Id;
            });
            if (questInProfile is not null)
            {
                quest.SptStatus = questInProfile.Status;
                questsToShowPlayer.Add(quest);
                continue;
            }

            // Filter out bear quests for USEC and vice versa
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
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"Unable to show quest: {quest.QuestName} as its for a trader: {quest.TraderId} that no longer exists."
                    );
                }

                continue;
            }

            var questRequirements = _questConditionHelper.GetQuestConditions(
                quest.Conditions.AvailableForStart
            );
            var loyaltyRequirements = _questConditionHelper.GetLoyaltyConditions(
                quest.Conditions.AvailableForStart
            );
            var standingRequirements = _questConditionHelper.GetStandingConditions(
                quest.Conditions.AvailableForStart
            );

            // Quest has no conditions, standing or loyalty conditions, add to visible quest list
            if (
                questRequirements.Count == 0
                && loyaltyRequirements.Count == 0
                && standingRequirements.Count == 0
            )
            {
                quest.SptStatus = QuestStatusEnum.AvailableForStart;
                questsToShowPlayer.Add(quest);
                continue;
            }

            // Check the status of each quest condition, if any are not completed
            // then this quest should not be visible
            var haveCompletedPreviousQuest = true;
            foreach (var conditionToFulfil in questRequirements)
            {
                // If the previous quest isn't in the user profile, it hasn't been completed or started
                var questIdsToFulfil =
                    (
                        conditionToFulfil.Target.IsList ? conditionToFulfil.Target.List
                        : conditionToFulfil.Target.Item == null ? null
                        : [conditionToFulfil.Target.Item]
                    ) ?? [];
                var prerequisiteQuest = profile.Quests.FirstOrDefault(profileQuest =>
                {
                    return questIdsToFulfil.Contains(profileQuest.QId);
                });

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
                    prerequisiteQuest.StatusTimers.TryGetValue(
                        prerequisiteQuest.Status.Value,
                        out var previousQuestCompleteTime
                    );
                    var unlockTime = previousQuestCompleteTime + conditionToFulfil.AvailableAfter;
                    if (unlockTime > _timeUtil.GetTimeStamp())
                    {
                        _logger.Debug(
                            $"Quest {quest.QuestName} is locked for another: {unlockTime - _timeUtil.GetTimeStamp()} seconds"
                        );
                    }
                }
            }

            // Previous quest not completed, skip
            if (!haveCompletedPreviousQuest)
            {
                continue;
            }

            var passesLoyaltyRequirements = true;
            foreach (var condition in loyaltyRequirements)
            {
                if (!TraderLoyaltyLevelRequirementCheck(condition, profile))
                {
                    passesLoyaltyRequirements = false;
                    break;
                }
            }

            var passesStandingRequirements = true;
            foreach (var condition in standingRequirements)
            {
                if (!TraderStandingRequirementCheck(condition, profile))
                {
                    passesStandingRequirements = false;
                    break;
                }
            }

            if (
                haveCompletedPreviousQuest
                && passesLoyaltyRequirements
                && passesStandingRequirements
            )
            {
                quest.SptStatus = QuestStatusEnum.AvailableForStart;
                questsToShowPlayer.Add(quest);
            }
        }

        return UpdateQuestsForGameEdition(questsToShowPlayer, profile.Info.GameVersion);
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
        var modifiedQuests = _cloner.Clone(quests);
        foreach (var quest in modifiedQuests)
        {
            // Remove any reward that doesn't pass the game edition check
            var propsAsDict = quest.Rewards.GetAllPropsAsDict();
            foreach (var rewardType in propsAsDict)
            {
                if (rewardType.Value is null)
                {
                    continue;
                }

                propsAsDict[rewardType.Key] = ((List<Reward>)propsAsDict[rewardType.Key])
                    .Where(reward =>
                    {
                        return _rewardHelper.RewardIsForGameEdition(reward, gameVersion);
                    })
                    .ToList();
            }
        }

        return modifiedQuests;
    }

    /**
     * Return a list of quests that would fail when supplied quest is completed
     * @param completedQuestId Quest completed id
     * @returns Array of Quest objects
     */
    protected List<Quest> GetQuestsFromProfileFailedByCompletingQuest(
        string completedQuestId,
        PmcData pmcProfile
    )
    {
        var questsInDb = GetQuestsFromDb();
        return questsInDb
            .Where(quest =>
            {
                // No fail conditions, skip
                if (quest.Conditions.Fail is null || quest.Conditions.Fail.Count == 0)
                {
                    return false;
                }

                // Quest already failed in profile, skip
                if (
                    pmcProfile.Quests.Any(profileQuest =>
                    {
                        return profileQuest.QId == quest.Id
                            && profileQuest.Status == QuestStatusEnum.Fail;
                    })
                )
                {
                    return false;
                }

                return quest.Conditions.Fail.Any(condition =>
                {
                    return condition.Target?.List?.Contains(completedQuestId) ?? false;
                });
            })
            .ToList();
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
        foreach (var questToFail in questsToFail)
        {
            // Skip failing a quest that has a fail status of something other than success
            if (
                questToFail.Conditions.Fail?.Any(x =>
                {
                    return x.Status?.Any(status =>
                        {
                            return status != QuestStatusEnum.Success;
                        }) ?? false;
                }) ?? false
            )
            {
                continue;
            }

            var isActiveQuestInPlayerProfile = pmcData.Quests.FirstOrDefault(quest =>
            {
                return quest.QId == questToFail.Id;
            });
            if (isActiveQuestInPlayerProfile is not null)
            {
                if (isActiveQuestInPlayerProfile.Status != QuestStatusEnum.Fail)
                {
                    var failBody = new FailQuestRequestData
                    {
                        Action = "QuestFail",
                        QuestId = questToFail.Id,
                        RemoveExcessItems = true,
                    };
                    FailQuest(pmcData, failBody, sessionID, output);
                }
            }
            else
            {
                // Failing an entirely new quest that doesn't exist in profile
                var statusTimers = new Dictionary<QuestStatusEnum, double>();

                if (!statusTimers.TryGetValue(QuestStatusEnum.Fail, out _))
                {
                    statusTimers.Add(QuestStatusEnum.Fail, 0);
                }

                statusTimers[QuestStatusEnum.Fail] = _timeUtil.GetTimeStamp();
                var questData = new QuestStatus
                {
                    QId = questToFail.Id,
                    StartTime = _timeUtil.GetTimeStamp(),
                    StatusTimers = statusTimers,
                    Status = QuestStatusEnum.Fail,
                };
                pmcData.Quests.Add(questData);
            }
        }
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
        var quest = GetQuestFromDb(completedQuestId, pmcData);

        _mailSendService.SendLocalisedNpcMessageToPlayer(
            sessionID,
            quest.TraderId,
            MessageType.QUEST_SUCCESS,
            quest.SuccessMessageText,
            questRewards,
            _timeUtil.GetHoursAsSeconds((int)GetMailItemRedeemTimeHoursForProfile(pmcData))
        );
    }

    /**
     * Look for newly available quests after completing a quest with a requirement to wait x minutes (time-locked) before being available and add data to profile
     * @param pmcData Player profile to update
     * @param quests Quests to look for wait conditions in
     * @param completedQuestId Quest just completed
     */
    protected void AddTimeLockedQuestsToProfile(
        PmcData pmcData,
        List<Quest> quests,
        string completedQuestId
    )
    {
        // Iterate over quests, look for quests with right criteria
        foreach (var quest in quests)
        {
            // If quest has prereq of completed quest + availableAfter value > 0 (quest has wait time)
            var nextQuestWaitCondition = quest.Conditions?.AvailableForStart?.FirstOrDefault(x =>
            {
                return (
                        (x.Target?.List?.Contains(completedQuestId) ?? false)
                        || (x.Target?.Item?.Contains(completedQuestId) ?? false)
                    )
                    && x.AvailableAfter > 0;
            }); // as we have to use the ListOrT type now, check both List and Item for the above checks

            if (nextQuestWaitCondition is not null)
            {
                // Now + wait time
                var availableAfterTimestamp =
                    _timeUtil.GetTimeStamp() + nextQuestWaitCondition.AvailableAfter;

                // Update quest in profile with status of AvailableAfter
                var existingQuestInProfile = pmcData.Quests.FirstOrDefault(x =>
                {
                    return x.QId == quest.Id;
                });
                if (existingQuestInProfile is not null)
                {
                    existingQuestInProfile.AvailableAfter = availableAfterTimestamp;
                    existingQuestInProfile.Status = QuestStatusEnum.AvailableAfter;
                    existingQuestInProfile.StartTime = 0;
                    existingQuestInProfile.StatusTimers = new Dictionary<QuestStatusEnum, double>();

                    continue;
                }

                pmcData.Quests.Add(
                    new QuestStatus
                    {
                        QId = quest.Id,
                        StartTime = 0,
                        Status = QuestStatusEnum.AvailableAfter,
                        StatusTimers = new Dictionary<QuestStatusEnum, double>
                        {
                            { QuestStatusEnum.AvailableAfter, _timeUtil.GetTimeStamp() },
                        },
                        AvailableAfter = availableAfterTimestamp,
                    }
                );
            }
        }
    }

    /**
     * Remove a quest entirely from a profile
     * @param sessionId Player id
     * @param questIdToRemove Qid of quest to remove
     */
    protected void RemoveQuestFromScavProfile(string sessionId, string questIdToRemove)
    {
        var fullProfile = _profileHelper.GetFullProfile(sessionId);
        var repeatableInScavProfile = fullProfile.CharacterData.ScavData.Quests?.FirstOrDefault(x =>
        {
            return x.QId == questIdToRemove;
        });
        if (repeatableInScavProfile is null)
        {
            _logger.Warning(
                _localisationService.GetText(
                    "quest-unable_to_remove_scav_quest_from_profile",
                    new { scavQuestId = questIdToRemove, profileId = sessionId }
                )
            );

            return;
        }

        fullProfile.CharacterData.ScavData.Quests.Remove(repeatableInScavProfile);
    }

    /**
     * Return quests that have different statuses
     * @param preQuestStatusus Quests before
     * @param postQuestStatuses Quests after
     * @returns QuestStatusChange array
     */
    protected List<QuestStatus> GetQuestsWithDifferentStatuses(
        List<QuestStatus> preQuestStatuses,
        List<QuestStatus> postQuestStatuses
    )
    {
        List<QuestStatus> result = [];

        foreach (var quest in postQuestStatuses)
        {
            // Add quest if status differs or quest not found
            var preQuest = preQuestStatuses.FirstOrDefault(x =>
            {
                return x.QId == quest.QId;
            });
            if (preQuest is null || preQuest.Status != quest.Status)
            {
                result.Add(quest);
            }
        }

        if (result.Count == 0)
        {
            return null;
        }

        return result;
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
        // No conditions
        {
            return true;
        }

        var levelConditions = _questConditionHelper.GetLevelConditions(
            quest.Conditions.AvailableForStart
        );
        if (levelConditions is not null)
        {
            foreach (var levelCondition in levelConditions)
            {
                if (!DoesPlayerLevelFulfilCondition(playerLevel, levelCondition))
                // Not valid, exit out
                {
                    return false;
                }
            }
        }

        // All conditions passed / has no level requirement, valid
        return true;
    }
}
