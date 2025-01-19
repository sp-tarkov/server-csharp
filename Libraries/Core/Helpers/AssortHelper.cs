using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using SptCommon.Extensions;

namespace Core.Helpers;

[Injectable]
public class AssortHelper(
    ISptLogger<AssortHelper> _logger,
    ItemHelper _itemHelper,
    DatabaseServer _databaseServer,
    LocalisationService _localisationService,
    QuestHelper _questHelper
)
{
    /**
     * Remove assorts from a trader that have not been unlocked yet (via player completing corresponding quest)
     * @param pmcProfile Player profile
     * @param traderId Traders id the assort belongs to
     * @param traderAssorts All assort items from same trader
     * @param mergedQuestAssorts Dict of quest assort to quest id unlocks for all traders (key = started/failed/complete)
     * @returns Assort items minus locked quest assorts
     */
    public TraderAssort StripLockedQuestAssort(
        PmcData pmcProfile,
        string traderId,
        TraderAssort traderAssorts,
        Dictionary<string, Dictionary<string, string>> mergedQuestAssorts,
        bool flea = false)
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
                strippedTraderAssorts = RemoveItemFromAssort(traderAssorts, assortId.Key, flea);
            }
        }

        return strippedTraderAssorts;
    }

    /**
     * Get a quest id + the statuses quest can be in to unlock assort
     * @param mergedQuestAssorts quest assorts to search for assort id
     * @param assortId Assort to look for linked quest id
     * @returns quest id + array of quest status the assort should show for
     */
    protected KeyValuePair<string, List<QuestStatusEnum>>? GetQuestIdAndStatusThatShowAssort(
        Dictionary<string, Dictionary<string, string>> mergedQuestAssorts,
        string assortId)
    {
        if (mergedQuestAssorts.Get<Dictionary<string, string>>("started").Contains(assortId))
        {
            // Assort unlocked by starting quest, assort is visible to player when : started or ready to hand in + handed in
            return new KeyValuePair<string, List<QuestStatusEnum>>(
                mergedQuestAssorts.Get<Dictionary<string, string>>("started")[assortId],
                [QuestStatusEnum.Started, QuestStatusEnum.AvailableForFinish, QuestStatusEnum.Success]
            );
        }

        if (mergedQuestAssorts.Get<Dictionary<string, string>>("success").Contains(assortId))
        {
            return new KeyValuePair<string, List<QuestStatusEnum>>(
                mergedQuestAssorts.Get<Dictionary<string, string>>("success")[assortId],
                [QuestStatusEnum.Success]
            );
        }

        if (mergedQuestAssorts.Get<Dictionary<string, string>>("fail").Contains(assortId))
        {
            return new KeyValuePair<string, List<QuestStatusEnum>>(
                mergedQuestAssorts.Get<Dictionary<string, string>>("success")[assortId],
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
            if (assort.LoyalLevelItems[item.Key] > pmcProfile.TradersInfo[traderId].LoyaltyLevel)
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

        if (assort.BarterScheme[itemID] is not null && flea)
        {
            foreach (var barterSchemes in assort.BarterScheme[itemID])
            {
                foreach (var barterScheme in barterSchemes)
                {
                    barterScheme.SptQuestLocked = true;
                }
            }

            return assort;
        }

        assort.BarterScheme.Remove(itemID);
        assort.LoyalLevelItems.Remove(itemID);

        foreach (var i in idsToRemove)
        {
            foreach (var a in assort.Items.ToList())
            {
                if (a.Id == i)
                {
                    assort.Items.Remove(a);
                }
            }
        }

        return assort;
    }
}
