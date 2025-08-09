using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class QuestController(
    ISptLogger<QuestController> logger,
    TimeUtil timeUtil,
    HttpResponseUtil httpResponseUtil,
    EventOutputHolder eventOutputHolder,
    MailSendService mailSendService,
    QuestHelper questHelper,
    QuestRewardHelper questRewardHelper,
    ServerLocalisationService serverLocalisationService,
    ICloner cloner
)
{
    /// <summary>
    ///     Handle client/quest/list
    ///     Get all quests visible to player
    ///     Exclude quests with incomplete preconditions (level/loyalty)
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>Collection of Quest</returns>
    public List<Quest> GetClientQuests(MongoId sessionId)
    {
        return questHelper.GetClientQuests(sessionId);
    }

    /// <summary>
    ///     Handle QuestAccept event
    ///     Handle the client accepting a quest and starting it
    ///     Send starting rewards if any to player and
    ///     Send start notification if any to player
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="acceptedQuest">Quest accepted</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse AcceptQuest(PmcData pmcData, AcceptQuestRequestData acceptedQuest, MongoId sessionID)
    {
        var acceptQuestResponse = eventOutputHolder.GetOutput(sessionID);

        // Does quest exist in profile
        // Restarting a failed quest can mean quest exists in profile
        var existingQuestStatus = pmcData.Quests.FirstOrDefault(x => x.QId == acceptedQuest.QuestId);
        if (existingQuestStatus is not null)
        {
            // Update existing
            questHelper.ResetQuestState(pmcData, QuestStatusEnum.Started, acceptedQuest.QuestId);

            // Need to send client an empty list of completedConditions (Unsure if this does anything)
            acceptQuestResponse.ProfileChanges[sessionID].QuestsStatus.Add(existingQuestStatus);
        }
        else
        {
            // Add new quest to server profile
            var newQuest = questHelper.GetQuestReadyForProfile(pmcData, QuestStatusEnum.Started, acceptedQuest);
            pmcData.Quests.Add(newQuest);
        }

        // Create a dialog message for starting the quest.
        // Note that for starting quests, the correct locale field is "description", not "startedMessageText".
        var questFromDb = questHelper.GetQuestFromDb(acceptedQuest.QuestId, pmcData);

        if (questFromDb.Conditions?.AvailableForFinish is not null)
        {
            AddTaskConditionCountersToProfile(questFromDb.Conditions.AvailableForFinish, pmcData, acceptedQuest.QuestId);
        }

        // Get messageId of text to send to player as text message in game
        var messageId = questHelper.GetMessageIdForQuestStart(questFromDb.StartedMessageText, questFromDb.Description);

        // Apply non-item rewards to profile + return item rewards
        var startedQuestRewardItems = questRewardHelper.ApplyQuestReward(
            pmcData,
            acceptedQuest.QuestId,
            QuestStatusEnum.Started,
            sessionID,
            acceptQuestResponse
        );

        // Send started text + any starting reward items found above to player
        mailSendService.SendLocalisedNpcMessageToPlayer(
            sessionID,
            questFromDb.TraderId,
            MessageType.QuestStart,
            messageId,
            startedQuestRewardItems.ToList(),
            timeUtil.GetHoursAsSeconds((int)questHelper.GetMailItemRedeemTimeHoursForProfile(pmcData))
        );

        // Having accepted new quest, look for newly unlocked quests and inform client of them
        var newlyAccessibleQuests = questHelper.GetNewlyAccessibleQuestsWhenStartingQuest(acceptedQuest.QuestId, sessionID);
        if (newlyAccessibleQuests.Count > 0)
        {
            acceptQuestResponse.ProfileChanges[sessionID].Quests.AddRange(newlyAccessibleQuests);
        }

        return acceptQuestResponse;
    }

    /// <summary>
    ///     Add a quest condition counters to chosen profile
    ///     Currently only used to add `SellItemToTrader` conditions
    /// </summary>
    /// <param name="questConditionsToAdd">Conditions to iterate over and possibly add to profile</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="questId">Quest where conditions originated</param>
    protected void AddTaskConditionCountersToProfile(IEnumerable<QuestCondition> questConditionsToAdd, PmcData pmcData, MongoId questId)
    {
        foreach (var condition in questConditionsToAdd.Where(condition => condition.ConditionType == "SellItemToTrader"))
        {
            if (pmcData.TaskConditionCounters.TryGetValue(condition.Id, out _))
            {
                logger.Debug(
                    $"Condition counter: {condition.ConditionType} already exists for quest: {questId} in profile: {pmcData.SessionId}, skipping"
                );
                continue;
            }

            pmcData.TaskConditionCounters.Add(
                condition.Id,
                new TaskConditionCounter
                {
                    Id = condition.Id,
                    SourceId = questId,
                    Type = condition.ConditionType,
                    Value = 0,
                }
            );
        }
    }

    /// <summary>
    ///     Handle QuestComplete event
    ///     Update completed quest in profile
    ///     Add newly unlocked quests to profile
    ///     Also recalculate their level due to exp rewards
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Complete quest request</param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse CompleteQuest(PmcData pmcData, CompleteQuestRequestData request, MongoId sessionId)
    {
        return questHelper.CompleteQuest(pmcData, request, sessionId);
    }

    /// <summary>
    ///     Handle QuestHandover event
    ///     Player hands over an item to trader to complete/partially complete quest
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Handover request</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse HandoverQuest(PmcData pmcData, HandoverQuestRequestData request, MongoId sessionID)
    {
        var quest = questHelper.GetQuestFromDb(request.QuestId, pmcData);
        HashSet<string> handoverQuestTypes = ["HandoverItem", "WeaponAssembly"];
        var output = eventOutputHolder.GetOutput(sessionID);

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
            isItemHandoverQuest = condition.ConditionType == handoverQuestTypes.FirstOrDefault(); // TODO: there's 2 values, why does it only check for the first
            handoverRequirements = condition;

            if (pmcData.TaskConditionCounters.TryGetValue(request.ConditionId, out var counter))
            {
                handedInCount -= (int)(counter.Value ?? 0);

                if (handedInCount <= 0)
                {
                    logger.Error(
                        serverLocalisationService.GetText(
                            "repeatable-quest_handover_failed_condition_already_satisfied",
                            new
                            {
                                questId = request.QuestId,
                                conditionId = request.ConditionId,
                                profileCounter = counter.Value,
                                value = handedInCount,
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
                return ShowQuestItemHandoverMatchError(request, matchingItemInProfile, handoverRequirements, output);
            }

            // Remove the right quantity of given items
            var itemCountToRemove = Math.Min(itemHandover.Count ?? 0, handedInCount - totalItemCountToRemove);
            totalItemCountToRemove += itemCountToRemove;
            if (itemHandover.Count - itemCountToRemove > 0)
            {
                // Remove single item with no children
                questHelper.ChangeItemStack(pmcData, itemHandover.Id, (int)(itemHandover.Count - itemCountToRemove), sessionID, output);

                // Complete - handedInCount == totalItemCountToRemove
                if (Math.Abs(totalItemCountToRemove - handedInCount) < 0.01)
                {
                    break;
                }
            }
            else
            {
                // Remove item with children
                var toRemove = pmcData.Inventory.Items.GetItemWithChildrenTpls(itemHandover.Id).ToHashSet();
                var index = pmcData.Inventory.Items.Count;

                // Important: don't tell the client to remove the attachments, it will handle it
                output.ProfileChanges[sessionID].Items.DeletedItems.Add(new DeletedItem { Id = itemHandover.Id });

                // Important: loop backward when removing items from the array we're looping on
                while (index-- > 0)
                {
                    if (toRemove.Contains(pmcData.Inventory.Items[index].Id))
                    {
                        var removedItem = cloner.Clone(pmcData.Inventory.Items[index]);
                        pmcData.Inventory.Items.RemoveAt(index);

                        // Remove the item
                        // If the removed item has a numeric `location` property, re-calculate all the child
                        // element `location` properties of the parent so they are sequential, while retaining order
                        if (removedItem.Location?.GetType() == typeof(int))
                        {
                            var childItems = pmcData.Inventory.Items.GetItemWithChildren(removedItem.ParentId);
                            childItems.RemoveAt(0); // Remove the parent

                            // Sort by the current `location` and update
                            childItems.Sort((a, b) => (int)a.Location > (int)b.Location ? 1 : -1);

                            for (var i = 0; i < childItems.Count; i++)
                            {
                                childItems[i].Location = i;
                            }
                        }
                    }
                }
            }
        }

        UpdateProfileTaskConditionCounterValue(pmcData, request.ConditionId, request.QuestId, totalItemCountToRemove);

        return output;
    }

    /// <summary>
    ///     Show warning to user and write to log that repeatable quest failed a condition check
    /// </summary>
    /// <param name="questId">Quest id that failed</param>
    /// <param name="conditionId">Relevant condition id that failed</param>
    /// <param name="output">Client response</param>
    /// <returns>ItemEventRouterResponse</returns>
    protected ItemEventRouterResponse ShowRepeatableQuestInvalidConditionError(
        MongoId questId,
        MongoId conditionId,
        ItemEventRouterResponse output
    )
    {
        var errorMessage = serverLocalisationService.GetText(
            "repeatable-quest_handover_failed_condition_invalid",
            new { questId, conditionId }
        );
        logger.Error(errorMessage);

        return httpResponseUtil.AppendErrorToOutput(output, errorMessage);
    }

    /// <summary>
    ///     Show warning to user and write to log quest item handed over did not match what is required
    /// </summary>
    /// <param name="handoverQuestRequest">Handover request</param>
    /// <param name="itemHandedOver">Non-matching item found</param>
    /// <param name="handoverRequirements">Quest handover requirements</param>
    /// <param name="output">Response to send to user</param>
    /// <returns>ItemEventRouterResponse</returns>
    protected ItemEventRouterResponse ShowQuestItemHandoverMatchError(
        HandoverQuestRequestData handoverQuestRequest,
        Item? itemHandedOver,
        QuestCondition? handoverRequirements,
        ItemEventRouterResponse output
    )
    {
        var errorMessage = serverLocalisationService.GetText(
            "quest-handover_wrong_item",
            new
            {
                questId = handoverQuestRequest.QuestId,
                handedInTpl = itemHandedOver?.Template ?? "UNKNOWN",
                requiredTpl = handoverRequirements.Target.List.FirstOrDefault(),
            }
        );
        logger.Error(errorMessage);

        return httpResponseUtil.AppendErrorToOutput(output, errorMessage);
    }

    /// <summary>
    ///     Increment a backend counter stored value by an amount
    ///     Create counter if it does not exist
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="conditionId">Backend counter id to update</param>
    /// <param name="questId">Quest id counter is associated with</param>
    /// <param name="counterValue">Value to increment the backend counter with</param>
    protected void UpdateProfileTaskConditionCounterValue(PmcData pmcData, MongoId conditionId, MongoId questId, double counterValue)
    {
        if (pmcData.TaskConditionCounters.GetValueOrDefault(conditionId) != null)
        {
            pmcData.TaskConditionCounters[conditionId].Value += counterValue;

            return;
        }

        pmcData.TaskConditionCounters.Add(
            conditionId,
            new TaskConditionCounter
            {
                Id = conditionId,
                SourceId = questId,
                Type = "HandoverItem",
                Value = counterValue,
            }
        );
    }

    /// <summary>
    ///     Handle /client/game/profile/items/moving - QuestFail
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Fail quest request</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="output"></param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse FailQuest(
        PmcData pmcData,
        FailQuestRequestData request,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        questHelper.FailQuest(pmcData, request, sessionID, output);

        return output;
    }
}
