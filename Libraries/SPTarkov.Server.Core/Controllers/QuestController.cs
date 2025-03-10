using SPTarkov.Server.Core.Helpers;
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
using SPTarkov.Common.Annotations;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;


namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class QuestController(
    ISptLogger<QuestController> _logger,
    TimeUtil _timeUtil,
    HttpResponseUtil _httpResponseUtil,
    EventOutputHolder _eventOutputHolder,
    DatabaseService _databaseService,
    ItemHelper _itemHelper,
    DialogueHelper _dialogueHelper,
    MailSendService _mailSendService,
    ProfileHelper _profileHelper,
    TraderHelper _traderHelper,
    QuestHelper _questHelper,
    QuestRewardHelper _questRewardHelper,
    QuestConditionHelper _questConditionHelper,
    PlayerService _playerService,
    LocaleService _localeService,
    LocalisationService _localisationService,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected QuestConfig _questConfig = _configServer.GetConfig<QuestConfig>();
    protected static readonly List<string> _questTypes = ["PickUp", "Exploration", "Elimination"];

    /// <summary>
    /// Handle client/quest/list
    /// Get all quests visible to player
    /// Exclude quests with incomplete preconditions (level/loyalty)
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>Collection of Quest</returns>
    public List<Quest> GetClientQuests(string sessionId)
    {
        return _questHelper.GetClientQuests(sessionId);
    }

    /// <summary>
    /// Handle QuestAccept event
    /// Handle the client accepting a quest and starting it
    /// Send starting rewards if any to player and
    /// Send start notification if any to player
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="acceptedQuest">Quest accepted</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse AcceptQuest(PmcData pmcData, AcceptQuestRequestData acceptedQuest, string sessionID)
    {
        var acceptQuestResponse = _eventOutputHolder.GetOutput(sessionID);

        // Does quest exist in profile
        // Restarting a failed quest can mean quest exists in profile
        var existingQuestStatus = pmcData.Quests.FirstOrDefault(x => x.QId == acceptedQuest.QuestId);
        if (existingQuestStatus is not null)
        {
            // Update existing
            _questHelper.ResetQuestState(pmcData, QuestStatusEnum.Started, acceptedQuest.QuestId);

            // Need to send client an empty list of completedConditions (Unsure if this does anything)
            acceptQuestResponse.ProfileChanges[sessionID].QuestsStatus.Add(existingQuestStatus);
        }
        else
        {
            // Add new quest to server profile
            var newQuest = _questHelper.GetQuestReadyForProfile(pmcData, QuestStatusEnum.Started, acceptedQuest);
            pmcData.Quests.Add(newQuest);
        }

        // Create a dialog message for starting the quest.
        // Note that for starting quests, the correct locale field is "description", not "startedMessageText".
        var questFromDb = _questHelper.GetQuestFromDb(acceptedQuest.QuestId, pmcData);

        AddTaskConditionCountersToProfile(questFromDb.Conditions.AvailableForFinish, pmcData, acceptedQuest.QuestId);

        // Get messageId of text to send to player as text message in game
        var messageId = _questHelper.GetMessageIdForQuestStart(
            questFromDb.StartedMessageText,
            questFromDb.Description
        );

        // Apply non-item rewards to profile + return item rewards
        var startedQuestRewardItems = _questRewardHelper.ApplyQuestReward(
            pmcData,
            acceptedQuest.QuestId,
            QuestStatusEnum.Started,
            sessionID,
            acceptQuestResponse
        );

        // Send started text + any starting reward items found above to player
        _mailSendService.SendLocalisedNpcMessageToPlayer(
            sessionID,
            _traderHelper.GetTraderById(questFromDb.TraderId).ToString(),
            MessageType.QUEST_START,
            messageId,
            startedQuestRewardItems.ToList(),
            _timeUtil.GetHoursAsSeconds((int) _questHelper.GetMailItemRedeemTimeHoursForProfile(pmcData))
        );

        // Having accepted new quest, look for newly unlocked quests and inform client of them
        var newlyAccessibleQuests = _questHelper.GetNewlyAccessibleQuestsWhenStartingQuest(
            acceptedQuest.QuestId,
            sessionID
        );
        if (newlyAccessibleQuests.Count > 0)
        {
            acceptQuestResponse.ProfileChanges[sessionID].Quests.AddRange(newlyAccessibleQuests);
        }

        return acceptQuestResponse;
    }

    /// <summary>
    /// Add a quests condition counters to chosen profile
    /// </summary>
    /// <param name="questConditions">Conditions to iterate over and possibly add to profile</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="questId">Quest where conditions originated</param>
    protected void AddTaskConditionCountersToProfile(List<QuestCondition>? questConditions, PmcData pmcData, string questId)
    {
        foreach (var condition in questConditions)
        {
            if (pmcData.TaskConditionCounters.TryGetValue(condition.Id, out var counter))
            {
                _logger.Error(
                    $"Unable to add new task condition counter: {condition.ConditionType} for quest: {questId} to profile: {pmcData.SessionId} as it already exists:"
                );
            }

            switch (condition.ConditionType)
            {
                case "SellItemToTrader":
                    pmcData.TaskConditionCounters[condition.Id] = new TaskConditionCounter
                    {
                        Id = condition.Id,
                        SourceId = questId,
                        Type = condition.ConditionType,
                        Value = 0
                    };
                    break;
            }
        }
    }

    /// <summary>
    /// TODO: Move this code into RepeatableQuestController
    /// Handle the client accepting a repeatable quest and starting it
    /// Send starting rewards if any to player and
    /// Send start notification if any to player
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="acceptedQuest">Repeatable quest accepted</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse AcceptRepeatableQuest(PmcData pmcData, AcceptQuestRequestData acceptedQuest, string sessionID)
    {
        // Create and store quest status object inside player profile
        var newRepeatableQuest = _questHelper.GetQuestReadyForProfile(
            pmcData,
            QuestStatusEnum.Started,
            acceptedQuest
        );
        pmcData.Quests.Add(newRepeatableQuest);

        // Look for the generated quest cache in profile.RepeatableQuests
        var repeatableQuestProfile = GetRepeatableQuestFromProfile(pmcData, acceptedQuest.QuestId);
        if (repeatableQuestProfile is null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "repeatable-accepted_repeatable_quest_not_found_in_active_quests",
                    acceptedQuest.QuestId
                )
            );

            throw new Exception(_localisationService.GetText("repeatable-unable_to_accept_quest_see_log"));
        }

        // Some scav quests need to be added to scav profile for them to show up in-raid
        if (repeatableQuestProfile.Side == "Scav" && _questTypes.Contains(repeatableQuestProfile.Type.ToString()))
        {
            var fullProfile = _profileHelper.GetFullProfile(sessionID);

            fullProfile.CharacterData.ScavData.Quests ??= [];
            fullProfile.CharacterData.ScavData.Quests.Add(newRepeatableQuest);
        }

        var response = _eventOutputHolder.GetOutput(sessionID);

        return response;
    }

    /// <summary>
    /// Look for an accepted quest inside player profile, return quest that matches
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="questId">Quest id to return</param>
    /// <returns>RepeatableQuest</returns>
    protected RepeatableQuest GetRepeatableQuestFromProfile(PmcData pmcData, string questId)
    {
        foreach (var repeatableQuest in pmcData.RepeatableQuests)
        {
            var matchingQuest = repeatableQuest.ActiveQuests.FirstOrDefault(x => x.Id == questId);
            if (matchingQuest is not null)
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"Accepted repeatable quest: {questId} from: {repeatableQuest.Name}");
                }

                matchingQuest.SptRepatableGroupName = repeatableQuest.Name;

                return matchingQuest;
            }
        }

        return null;
    }

    /// <summary>
    /// Handle QuestComplete event
    /// Update completed quest in profile
    /// Add newly unlocked quests to profile
    /// Also recalculate their level due to exp rewards
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Complete quest request</param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse CompleteQuest(PmcData pmcData, CompleteQuestRequestData request, string sessionId)
    {
        return _questHelper.CompleteQuest(pmcData, request, sessionId);
    }

    /// <summary>
    /// Handle QuestHandover event
    /// Player hands over an item to trader to complete/partially complete quest
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Handover request</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse HandoverQuest(PmcData pmcData, HandoverQuestRequestData request, string sessionID)
    {
        var quest = _questHelper.GetQuestFromDb(request.QuestId, pmcData);
        List<string> handoverQuestTypes = ["HandoverItem", "WeaponAssembly"];
        var output = _eventOutputHolder.GetOutput(sessionID);

        var isItemHandoverQuest = true;
        var handedInCount = 0;

        // Decrement number of items handed in
        QuestCondition? handoverRequirements = null;
        foreach (var condition in quest.Conditions.AvailableForFinish.Where(condition => condition.Id == request.ConditionId))
        {
            // Not a handover quest type, skip
            if (!handoverQuestTypes.Contains(condition.ConditionType))
            {
                continue;
            }

            handedInCount = int.Parse(condition.Value.ToString());
            isItemHandoverQuest = condition.ConditionType == handoverQuestTypes.FirstOrDefault();
            handoverRequirements = condition;

            if (pmcData.TaskConditionCounters.TryGetValue("ConditionId", out var counter))
            {
                handedInCount -= (int) (counter.Value ?? 0);

                if (handedInCount <= 0)
                {
                    _logger.Error(
                        _localisationService.GetText(
                            "repeatable-quest_handover_failed_condition_already_satisfied",
                            new
                            {
                                questId = request.QuestId,
                                conditionId = request.ConditionId,
                                profileCounter = counter.Value,
                                value = handedInCount
                            }
                        )
                    );

                    return output;
                }

                break;
            }
        }

        if (isItemHandoverQuest && handedInCount == 0)
        {
            return ShowRepeatableQuestInvalidConditionError(request.QuestId, request.ConditionId, output);
        }

        var totalItemCountToRemove = 0d;
        foreach (var itemHandover in request.Items)
        {
            var matchingItemInProfile = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == itemHandover.Id);
            if (!(matchingItemInProfile is not null && handoverRequirements.Target.List.Contains(matchingItemInProfile.Template)))
                // Item handed in by player doesn't match what was requested
            {
                return ShowQuestItemHandoverMatchError(
                    request,
                    matchingItemInProfile,
                    handoverRequirements,
                    output
                );
            }

            // Remove the right quantity of given items
            var itemCountToRemove = Math.Min(itemHandover.Count ?? 0, handedInCount - totalItemCountToRemove);
            totalItemCountToRemove += itemCountToRemove;
            if (itemHandover.Count - itemCountToRemove > 0)
            {
                // Remove single item with no children
                _questHelper.ChangeItemStack(
                    pmcData,
                    itemHandover.Id.Value,
                    (int) (itemHandover.Count - itemCountToRemove),
                    sessionID,
                    output
                );

                // Complete - handedInCount == totalItemCountToRemove
                if (Math.Abs(totalItemCountToRemove - handedInCount) < 0.01)
                {
                    break;
                }
            }
            else
            {
                // Remove item with children
                var toRemove = _itemHelper.FindAndReturnChildrenByItems(pmcData.Inventory.Items, itemHandover.Id);
                var index = pmcData.Inventory.Items.Count;

                // Important: don't tell the client to remove the attachments, it will handle it
                output.ProfileChanges[sessionID]
                    .Items.DeletedItems.Add(
                        new Item
                        {
                            Id = itemHandover.Id
                        }
                    );

                // Important: loop backward when removing items from the array we're looping on
                while (index-- > 0)
                {
                    if (toRemove.Contains(pmcData.Inventory.Items[index].Id))
                    {
                        var removedItem = _cloner.Clone(pmcData.Inventory.Items[index]);
                        pmcData.Inventory.Items.RemoveAt(index);

                        // Remove the item
                        // If the removed item has a numeric `location` property, re-calculate all the child
                        // element `location` properties of the parent so they are sequential, while retaining order
                        if (removedItem.Location?.GetType() == typeof(int))
                        {
                            var childItems = _itemHelper.FindAndReturnChildrenAsItems(
                                pmcData.Inventory.Items,
                                removedItem.ParentId
                            );
                            childItems.RemoveAt(0); // Remove the parent

                            // Sort by the current `location` and update
                            childItems.Sort((a, b) => (int) a.Location > (int) b.Location ? 1 : -1);

                            for (var i = 0; i < childItems.Count; i++)
                            {
                                childItems[i].Location = i;
                            }
                        }
                    }
                }
            }
        }

        UpdateProfileTaskConditionCounterValue(
            pmcData,
            request.ConditionId,
            request.QuestId,
            totalItemCountToRemove
        );

        return output;
    }

    /// <summary>
    /// Show warning to user and write to log that repeatable quest failed a condition check
    /// </summary>
    /// <param name="questId">Quest id that failed</param>
    /// <param name="conditionId">Relevant condition id that failed</param>
    /// <param name="output">Client response</param>
    /// <returns>ItemEventRouterResponse</returns>
    protected ItemEventRouterResponse ShowRepeatableQuestInvalidConditionError(string questId, string conditionId, ItemEventRouterResponse output)
    {
        var errorMessage = _localisationService.GetText(
            "repeatable-quest_handover_failed_condition_invalid",
            new
            {
                questId = questId,
                conditionId = conditionId
            }
        );
        _logger.Error(errorMessage);

        return _httpResponseUtil.AppendErrorToOutput(output, errorMessage);
    }

    /// <summary>
    /// Show warning to user and write to log quest item handed over did not match what is required
    /// </summary>
    /// <param name="handoverQuestRequest">Handover request</param>
    /// <param name="itemHandedOver">Non-matching item found</param>
    /// <param name="handoverRequirements">Quest handover requirements</param>
    /// <param name="output">Response to send to user</param>
    /// <returns>ItemEventRouterResponse</returns>
    protected ItemEventRouterResponse ShowQuestItemHandoverMatchError(HandoverQuestRequestData handoverQuestRequest, Item? itemHandedOver,
        QuestCondition? handoverRequirements, ItemEventRouterResponse output)
    {
        var errorMessage = _localisationService.GetText(
            "quest-handover_wrong_item",
            new
            {
                questId = handoverQuestRequest.QuestId,
                handedInTpl = itemHandedOver?.Template ?? "UNKNOWN",
                requiredTpl = handoverRequirements.Target.List.FirstOrDefault()
            }
        );
        _logger.Error(errorMessage);

        return _httpResponseUtil.AppendErrorToOutput(output, errorMessage);
    }

    /// <summary>
    /// Increment a backend counter stored value by an amount
    /// Create counter if it does not exist
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="conditionId">Backend counter id to update</param>
    /// <param name="questId">Quest id counter is associated with</param>
    /// <param name="counterValue">Value to increment the backend counter with</param>
    protected void UpdateProfileTaskConditionCounterValue(PmcData pmcData, string conditionId, string questId, double counterValue)
    {
        if (pmcData.TaskConditionCounters.GetValueOrDefault(conditionId) != null)
        {
            pmcData.TaskConditionCounters[conditionId].Value += counterValue;

            return;
        }

        pmcData.TaskConditionCounters.Add(conditionId, new TaskConditionCounter
        {
            Id = conditionId,
            SourceId = questId,
            Type = "HandoverItem",
            Value = counterValue
        });
    }

    /// <summary>
    /// Handle /client/game/profile/items/moving - QuestFail
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Fail quest request</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="output"></param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse FailQuest(PmcData pmcData, FailQuestRequestData request, string sessionID, ItemEventRouterResponse output)
    {
        _questHelper.FailQuest(pmcData, request, sessionID, output);

        return output;
    }
}
