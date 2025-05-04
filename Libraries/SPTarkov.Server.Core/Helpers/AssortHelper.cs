using SPTarkov.Common.Annotations;
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
    /// <param name="flea">TODO: what is this for</param>
    /// <returns>items minus locked quest assorts</returns>
    public TraderAssort StripLockedQuestAssort(
        PmcData pmcProfile,
        string traderId,
        TraderAssort traderAssorts,
        Dictionary<string, Dictionary<string, string>> mergedQuestAssorts,
        bool flea = false
    )
    {
        var strippedTraderAssorts = traderAssorts;

        // Trader assort does not always contain loyal_level_items
        if (traderAssorts.LoyalLevelItems is null)
        {
            _logger.Warning(
                _localisationService.GetText("assort-missing_loyalty_level_object", traderId)
            );

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
            var questStatusInProfile = _questHelper.GetQuestStatus(
                pmcProfile,
                unlockValues.Value.Key
            );
            if (!unlockValues.Value.Value.Contains(questStatusInProfile))
            {
                strippedTraderAssorts = RemoveItemFromAssort(traderAssorts, assortId.Key, flea);
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
        string assortId
    )
    {
        if (mergedQuestAssorts.TryGetValue("started", out var dict1) && dict1.ContainsKey(assortId))
        // Assort unlocked by starting quest, assort is visible to player when : started or ready to hand in + handed in
        {
            return new KeyValuePair<string, List<QuestStatusEnum>>(
                mergedQuestAssorts["started"][assortId],
                [
                    QuestStatusEnum.Started,
                    QuestStatusEnum.AvailableForFinish,
                    QuestStatusEnum.Success,
                ]
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

    /**
     * Remove assorts from a trader that have not been unlocked yet
     * @param pmcProfile player profile
     * @param traderId traders id
     * @param assort traders assorts
     * @returns traders assorts minus locked loyalty assorts
     */
    public TraderAssort StripLockedLoyaltyAssort(
        PmcData pmcProfile,
        string traderId,
        TraderAssort assort
    )
    {
        var strippedAssort = assort;

        // Trader assort does not always contain loyal_level_items
        if (assort.LoyalLevelItems is null)
        {
            _logger.Warning(
                _localisationService.GetText("assort-missing_loyalty_level_object", traderId)
            );

            return strippedAssort;
        }

        // Remove items restricted by loyalty levels above those reached by the player
        foreach (var item in assort.LoyalLevelItems)
        {
            if (
                pmcProfile.TradersInfo.TryGetValue(traderId, out var info)
                && assort.LoyalLevelItems[item.Key] > info.LoyaltyLevel
            )
            {
                strippedAssort = RemoveItemFromAssort(assort, item.Key);
            }
        }

        return strippedAssort;
    }

    /**
     * Remove an item from an assort
     * @param assort assort to modify
     * @param itemID item id to remove from assort
     * @returns Modified assort
     */
    public TraderAssort RemoveItemFromAssort(TraderAssort assort, string itemID, bool flea = false)
    {
        var idsToRemove = _itemHelper.FindAndReturnChildrenByItems(assort.Items, itemID);

        if (assort.BarterScheme.TryGetValue(itemID, out var lisToUse) && flea)
        {
            foreach (var barterScheme in lisToUse.SelectMany(barterSchemes => barterSchemes))
            {
                barterScheme.SptQuestLocked = true;
            }

            return assort;
        }

        assort.BarterScheme.Remove(itemID);
        assort.LoyalLevelItems.Remove(itemID);

        foreach (
            var item in idsToRemove.SelectMany(i => assort.Items.ToList().Where(a => a.Id == i))
        )
        {
            assort.Items.Remove(item);
        }

        return assort;
    }
}
