using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;

namespace Core.Helpers;

[Injectable]
public class AssortHelper
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
        List<TraderAssort> traderAssorts,
        Dictionary<string, Dictionary<string, string>> mergedQuestAssorts,
        bool flea = false)
    {
        throw new NotImplementedException();
    }

    /**
     * Get a quest id + the statuses quest can be in to unlock assort
     * @param mergedQuestAssorts quest assorts to search for assort id
     * @param assortId Assort to look for linked quest id
     * @returns quest id + array of quest status the assort should show for
     */
    protected (string questId, QuestStatus[] status) GetQuestIdAndStatusThatShowAssort(
        Dictionary<string, Dictionary<string, string>> mergedQuestAssorts,
        string assortId)
    {
        throw new NotImplementedException();
    }

    /**
     * Remove assorts from a trader that have not been unlocked yet
     * @param pmcProfile player profile
     * @param traderId traders id
     * @param assort traders assorts
     * @returns traders assorts minus locked loyalty assorts
     */
    public TraderAssort StripLockedLoyaltyAssort(PmcData pmcProfile, string traderId, List<TraderAssort> assort)
    {
        throw new NotImplementedException();
    }

    /**
     * Remove an item from an assort
     * @param assort assort to modify
     * @param itemID item id to remove from assort
     * @returns Modified assort
     */
    public TraderAssort RemoveItemFromAssort(List<TraderAssort> assort, string itemID, bool flea = false)
    {
        throw new NotImplementedException();
    }
}
