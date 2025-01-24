using SptCommon.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;


namespace Core.Controllers;

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
    protected List<string> _questTypes = ["PickUp", "Exploration", "Elimination"];

    public List<Quest> GetClientQuest(string sessionId)
    {
        return _questHelper.GetClientQuests(sessionId);
    }

    public ItemEventRouterResponse AcceptQuest(PmcData pmcData, AcceptQuestRequestData acceptedQuest, string sessionID)
    {
        var acceptQuestResponse = _eventOutputHolder.GetOutput(sessionID);

        // Does quest exist in profile
        // Restarting a failed quest can mean quest exists in profile
        var existingQuestStatus = pmcData.Quests.FirstOrDefault((x) => x.QId == acceptedQuest.QuestId);
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
            _timeUtil.GetHoursAsSeconds((int)_questHelper.GetMailItemRedeemTimeHoursForProfile(pmcData))
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

    private void AddTaskConditionCountersToProfile(List<QuestCondition>? questConditions, PmcData pmcData, string questId)
    {
        foreach (var condition in questConditions)
        {
            if (pmcData.TaskConditionCounters.TryGetValue(condition.Id, out var counter))
            {
                _logger.Error(
                    $"Unable to add new task condition counter: {condition.ConditionType} for qeust: {questId} to profile: {pmcData.SessionId} as it already exists:"
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
                        Value = 0,
                    };
                    break;
            }
        }
    }

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
        var repeatableQuestProfile = GetRepeatableQuestFromProfile(pmcData, acceptedQuest);
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
            if (fullProfile.CharacterData.ScavData.Quests is null)
            {
                fullProfile.CharacterData.ScavData.Quests = [];
            }

            fullProfile.CharacterData.ScavData.Quests.Add(newRepeatableQuest);
        }

        var response = _eventOutputHolder.GetOutput(sessionID);

        return response;
    }

    private RepeatableQuest GetRepeatableQuestFromProfile(PmcData pmcData, AcceptQuestRequestData acceptedQuest)
    {
        foreach (var repeatableQuest in pmcData.RepeatableQuests)
        {
            var matchingQuest = repeatableQuest.ActiveQuests.FirstOrDefault(x => x.Id == acceptedQuest.QuestId);
            if (matchingQuest is not null)
            {
                _logger.Debug($"Accepted repeatable quest {acceptedQuest.QuestId} from {repeatableQuest.Name}");
                matchingQuest.SptRepatableGroupName = repeatableQuest.Name;

                return matchingQuest;
            }
        }

        return null;
    }

    public ItemEventRouterResponse CompleteQuest(PmcData pmcData, CompleteQuestRequestData info, string sessionId)
    {
        return _questHelper.CompleteQuest(pmcData, info, sessionId);
    }


    public ItemEventRouterResponse HandoverQuest(PmcData pmcData, HandoverQuestRequestData handoverQuestRequest, string sessionID)
    {
        var quest = _questHelper.GetQuestFromDb(handoverQuestRequest.QuestId, pmcData);
        List<string> handoverQuestTypes = ["HandoverItem", "WeaponAssembly"];
        var output = _eventOutputHolder.GetOutput(sessionID);

        var isItemHandoverQuest = true;
        var handedInCount = 0;

        // Decrement number of items handed in
        QuestCondition? handoverRequirements = null;
        foreach (var condition in quest.Conditions.AvailableForFinish)
        {
            if (condition.Id == handoverQuestRequest.ConditionId && handoverQuestTypes.Contains(condition.ConditionType))
            {
                handedInCount = int.Parse((string)condition.Value);
                isItemHandoverQuest = condition.ConditionType == handoverQuestTypes.FirstOrDefault();
                handoverRequirements = condition;

                if (pmcData.TaskConditionCounters.TryGetValue("ConditionId", out var counter))
                {
                    handedInCount -= (int)(counter.Value ?? 0);

                    if (handedInCount <= 0)
                    {
                        _logger.Error(
                            _localisationService.GetText(
                                "repeatable-quest_handover_failed_condition_already_satisfied",
                                new
                                {
                                    questId = handoverQuestRequest.QuestId,
                                    conditionId = handoverQuestRequest.ConditionId,
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
        }

        if (isItemHandoverQuest && handedInCount == 0)
        {
            return ShowRepeatableQuestInvalidConditionError(handoverQuestRequest, output);
        }

        var totalItemCountToRemove = 0d;
        foreach (var itemHandover in handoverQuestRequest.Items)
        {
            var matchingItemInProfile = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == itemHandover.Id);
            if (!(matchingItemInProfile is not null && handoverRequirements.Target.List.Contains(matchingItemInProfile.Template)))
            {
                // Item handed in by player doesnt match what was requested
                return ShowQuestItemHandoverMatchError(
                    handoverQuestRequest,
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
                    itemHandover.Id,
                    (int)(itemHandover.Count - itemCountToRemove ?? 0),
                    sessionID,
                    output
                );
                if (totalItemCountToRemove == handedInCount)
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
                        if (removedItem.Location.GetType() == typeof(int))
                        {
                            var childItems = _itemHelper.FindAndReturnChildrenAsItems(
                                pmcData.Inventory.Items,
                                removedItem.ParentId
                            );
                            childItems.RemoveAt(0); // Remove the parent

                            // Sort by the current `location` and update
                            childItems.Sort((a, b) => (((int)a.Location) > ((int)b.Location) ? 1 : -1));

                            for (int i = 0; i < childItems.Count; i++)
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
            handoverQuestRequest.ConditionId,
            handoverQuestRequest.QuestId,
            totalItemCountToRemove
        );

        return output;
    }

    private ItemEventRouterResponse ShowRepeatableQuestInvalidConditionError(HandoverQuestRequestData handoverQuestRequest, ItemEventRouterResponse output)
    {
        var errorMessage = _localisationService.GetText(
            "repeatable-quest_handover_failed_condition_invalid",
            new
            {
                questId = handoverQuestRequest.QuestId,
                conditionId = handoverQuestRequest.ConditionId
            }
        );
        _logger.Error(errorMessage);

        return _httpResponseUtil.AppendErrorToOutput(output, errorMessage);
    }

    private ItemEventRouterResponse ShowQuestItemHandoverMatchError(HandoverQuestRequestData handoverQuestRequest, Item? itemHandedOver,
        QuestCondition? handoverRequirements, ItemEventRouterResponse output)
    {
        var errorMessage = _localisationService.GetText(
            "quest-handover_wrong_item",
            new
            {
                questId = handoverQuestRequest.QuestId,
                handedInTpl = itemHandedOver?.Template ?? "UNKNOWN",
                requiredTpl = handoverRequirements.Target.List.FirstOrDefault(),
            }
        );
        _logger.Error(errorMessage);

        return _httpResponseUtil.AppendErrorToOutput(output, errorMessage);
    }

    private void UpdateProfileTaskConditionCounterValue(PmcData pmcData, string conditionId, string questId, double counterValue)
    {
        if (pmcData.TaskConditionCounters[conditionId] != null)
        {
            pmcData.TaskConditionCounters[conditionId].Value += counterValue;

            return;
        }

        pmcData.TaskConditionCounters[conditionId] = new TaskConditionCounter
        {
            Id = conditionId,
            SourceId = questId,
            Type = "HandoverItem",
            Value = counterValue,
        };
    }

    public ItemEventRouterResponse FailQuest(PmcData pmcData, FailQuestRequestData request, string sessionID, ItemEventRouterResponse output)
    {
        _questHelper.FailQuest(pmcData, request, sessionID, output);

        return output;
    }
}
