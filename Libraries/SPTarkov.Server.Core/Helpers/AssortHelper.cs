using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class AssortHelper(
    ISptLogger<AssortHelper> _logger,
    ItemHelper _itemHelper,
    DatabaseServer _databaseServer,
    LocalisationService _localisationService,
    QuestHelper _questHelper
)
{
    /// <summary>
    ///     Remove assorts from a trader that have not been unlocked yet (via player completing corresponding quest)
    /// </summary>
    /// <param name="pmcProfile"></param>
    /// <param name="traderId">Traders id the assort belongs to</param>
    /// <param name="traderAssorts">All assort items from same trader</param>
    /// <param name="mergedQuestAssorts">Dict of quest assort to quest id unlocks for all traders (key = started/failed/complete)</param>
    /// <param name="isFlea">Is the trader assort being modified the flea market</param>
    /// <returns>items minus locked quest assorts</returns>
    public TraderAssort StripLockedQuestAssort(
        PmcData pmcProfile,
        string traderId,
        TraderAssort traderAssorts,
        Dictionary<string, Dictionary<string, string>> mergedQuestAssorts,
        bool isFlea = false)
    {
        var strippedTraderAssorts = traderAssorts;

        // Trader assort does not always contain loyal_level_items
        if (traderAssorts.LoyalLevelItems is null)
        {
            _logger.Warning(_localisationService.GetText("assort-missing_loyalty_level_object", traderId));

            return traderAssorts;
        }

        // Iterate over all assorts, removing items that haven't yet been unlocked by quests (ASSORTMENT_UNLOCK)
        foreach (var assortId in traderAssorts.LoyalLevelItems)
        {
            // Get quest id that unlocks assort + statuses quest can be in to show assort
            var unlockValues = GetQuestIdAndStatusThatShowAssort(mergedQuestAssorts, assortId.Key);
            if (unlockValues is null)
            {
                continue;
            }

            // Remove assort if quest in profile does not have status that unlocks assort
            var questStatusInProfile = _questHelper.GetQuestStatus(pmcProfile, unlockValues.Value.Key);
            if (!unlockValues.Value.Value.Contains(questStatusInProfile))
            {
                strippedTraderAssorts = RemoveItemFromAssort(traderAssorts, assortId.Key, isFlea);
            }
        }

        return strippedTraderAssorts;
    }

    /// <summary>
    ///     Get a quest id + the statuses quest can be in to unlock assort
    /// </summary>
    /// <param name="mergedQuestAssorts">quest assorts to search for assort id</param>
    /// <param name="assortId">Assort to look for linked quest id</param>
    /// <returns>quest id + array of quest status the assort should show for</returns>
    protected KeyValuePair<string, List<QuestStatusEnum>>? GetQuestIdAndStatusThatShowAssort(
        Dictionary<string, Dictionary<string, string>> mergedQuestAssorts,
        string assortId)
    {
        if (mergedQuestAssorts.TryGetValue("started", out var dict1) && dict1.ContainsKey(assortId))
            // Assort unlocked by starting quest, assort is visible to player when : started or ready to hand in + handed in
        {
            return new KeyValuePair<string, List<QuestStatusEnum>>(
                mergedQuestAssorts["started"][assortId],
                [QuestStatusEnum.Started, QuestStatusEnum.AvailableForFinish, QuestStatusEnum.Success]
            );
        }

        if (mergedQuestAssorts.TryGetValue("success", out var dict2) && dict2.ContainsKey(assortId))
        {
            return new KeyValuePair<string, List<QuestStatusEnum>>(
                mergedQuestAssorts["success"][assortId],
                [QuestStatusEnum.Success]
            );
        }

        if (mergedQuestAssorts.TryGetValue("fail", out var dict3) && dict3.ContainsKey(assortId))
        {
            return new KeyValuePair<string, List<QuestStatusEnum>>(
                mergedQuestAssorts["fail"][assortId],
                [QuestStatusEnum.Fail]
            );
        }

        return null;
    }

    /// <summary>
    /// Remove assorts from a trader that have not been unlocked yet
    /// </summary>
    /// <param name="pmcProfile">Player profile</param>
    /// <param name="traderId">Traders id</param>
    /// <param name="assort">Traders assorts</param>
    /// <returns>Trader assorts minus locked loyalty assorts</returns>
    public TraderAssort StripLockedLoyaltyAssort(PmcData pmcProfile, string traderId, TraderAssort assort)
    {
        var strippedAssort = assort;

        // Trader assort does not always contain loyal_level_items
        if (assort.LoyalLevelItems is null)
        {
            _logger.Warning(_localisationService.GetText("assort-missing_loyalty_level_object", traderId));

            return strippedAssort;
        }

        // Remove items restricted by loyalty levels above those reached by the player
        foreach (var item in assort.LoyalLevelItems)
        {
            if (pmcProfile.TradersInfo.TryGetValue(traderId, out var info) && assort.LoyalLevelItems[item.Key] > info.LoyaltyLevel)
            {
                strippedAssort = RemoveItemFromAssort(assort, item.Key);
            }
        }

        return strippedAssort;
    }

    /// <summary>
    /// Remove an item from an assort
    /// Must be removed from the assorts; items + barterScheme + LoyaltyLevel
    /// </summary>
    /// <param name="assort">Assort to remove item from</param>
    /// <param name="itemId">Id of item to remove from assort</param>
    /// <param name="isFlea">Is the assort being modified the flea market assort</param>
    /// <returns>Modified assort</returns>
    public TraderAssort RemoveItemFromAssort(TraderAssort assort, string itemId, bool isFlea = false)
    {
        // Flea assort needs special handling, item must remain in assort but be flagged as locked
        if (isFlea && assort.BarterScheme.TryGetValue(itemId, out var listToUse))
        {
            foreach (var barterScheme in listToUse.SelectMany(barterSchemes => barterSchemes))
            {
                barterScheme.SptQuestLocked = true;
            }

            return assort;
        }

        assort.BarterScheme.Remove(itemId);
        assort.LoyalLevelItems.Remove(itemId);

        // The item being removed may have children linked to it, find and remove them too
        var idsToRemove = _itemHelper.FindAndReturnChildrenByItems(assort.Items, itemId);
        assort.Items.RemoveAll(item => idsToRemove.Contains(item.Id));

        return assort;
    }
}
