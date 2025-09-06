using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class AssortHelper(ISptLogger<AssortHelper> logger, ServerLocalisationService serverLocalisationService)
{
    /// <summary>
    ///     Remove assorts from a trader that have not been unlocked yet (via player completing corresponding quest)
    /// </summary>
    /// <param name="pmcProfile"></param>
    /// <param name="traderId">Traders id assort belongs to</param>
    /// <param name="traderAssorts">All assort items from same trader</param>
    /// <param name="mergedQuestAssorts">Dict of quest assort to quest id unlocks for all traders (key = started/failed/complete)</param>
    /// <param name="isFlea">Is the trader assort being modified the flea market</param>
    /// <returns>items minus locked quest assorts</returns>
    public TraderAssort StripLockedQuestAssort(
        PmcData pmcProfile,
        MongoId traderId,
        TraderAssort traderAssorts,
        Dictionary<string, Dictionary<MongoId, MongoId>> mergedQuestAssorts,
        bool isFlea = false
    )
    {
        var strippedTraderAssorts = traderAssorts;

        // Trader assort does not always contain loyal_level_items
        if (traderAssorts.LoyalLevelItems is null)
        {
            logger.Warning(serverLocalisationService.GetText("assort-missing_loyalty_level_object", traderId));

            return traderAssorts;
        }

        // Iterate over all assorts, removing items that haven't yet been unlocked by quests (ASSORTMENT_UNLOCK)
        foreach (var (assortId, _) in traderAssorts.LoyalLevelItems)
        {
            // Get quest id that unlocks assort + statuses quest can be in to show assort
            var unlockValues = GetQuestIdAndStatusThatShowAssort(mergedQuestAssorts, assortId);
            if (unlockValues is null)
            {
                continue;
            }

            // Remove assort if quest in profile does not have status that unlocks assort
            var questStatusInProfile = pmcProfile.GetQuestStatus(unlockValues.Value.Key);
            if (!unlockValues.Value.Value.Contains(questStatusInProfile))
            {
                strippedTraderAssorts = traderAssorts.RemoveItemFromAssort(assortId, isFlea);
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
    protected KeyValuePair<MongoId, HashSet<QuestStatusEnum>>? GetQuestIdAndStatusThatShowAssort(
        Dictionary<string, Dictionary<MongoId, MongoId>> mergedQuestAssorts,
        MongoId assortId
    )
    {
        if (mergedQuestAssorts.TryGetValue("started", out var dict1) && dict1.ContainsKey(assortId))
        // Assort unlocked by starting quest, assort is visible to player when : started or ready to hand in + handed in
        {
            return new KeyValuePair<MongoId, HashSet<QuestStatusEnum>>(
                mergedQuestAssorts["started"][assortId],
                [QuestStatusEnum.Started, QuestStatusEnum.AvailableForFinish, QuestStatusEnum.Success]
            );
        }

        if (mergedQuestAssorts.TryGetValue("success", out var dict2) && dict2.ContainsKey(assortId))
        {
            return new KeyValuePair<MongoId, HashSet<QuestStatusEnum>>(mergedQuestAssorts["success"][assortId], [QuestStatusEnum.Success]);
        }

        if (mergedQuestAssorts.TryGetValue("fail", out var dict3) && dict3.ContainsKey(assortId))
        {
            return new KeyValuePair<MongoId, HashSet<QuestStatusEnum>>(mergedQuestAssorts["fail"][assortId], [QuestStatusEnum.Fail]);
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
    public TraderAssort StripLockedLoyaltyAssort(PmcData pmcProfile, MongoId traderId, TraderAssort assort)
    {
        var strippedAssort = assort;

        // Trader assort does not always contain loyal_level_items
        if (assort.LoyalLevelItems is null)
        {
            logger.Warning(serverLocalisationService.GetText("assort-missing_loyalty_level_object", traderId));

            return strippedAssort;
        }

        // Get trader info from profile
        // Assumption - Assort is for single trader only
        if (!pmcProfile.TradersInfo.TryGetValue(traderId, out var traderInfo))
        {
            return assort;
        }

        // Remove items restricted by loyalty levels above those reached by the player
        foreach (var item in assort.LoyalLevelItems.Where(item => assort.LoyalLevelItems[item.Key] > traderInfo.LoyaltyLevel))
        {
            strippedAssort = assort.RemoveItemFromAssort(item.Key);
        }

        return strippedAssort;
    }
}
