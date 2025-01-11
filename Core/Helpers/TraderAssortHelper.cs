using Core.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Helpers;

[Injectable]
public class TraderAssortHelper
{
    public TraderAssortHelper()
    {
        
    }
    
    /// <summary>
    /// Get a traders assorts
    /// Can be used for returning ragfair / fence assorts
    /// Filter out assorts not unlocked due to level OR quest completion
    /// </summary>
    /// <param name="sessionId">session id</param>
    /// <param name="traderId">traders id</param>
    /// <param name="showLockedAssorts">Should assorts player hasn't unlocked be returned - default false</param>
    /// <returns>a traders' assorts</returns>
    public TraderAssort GetAssort(string sessionId, string traderId, bool showLockedAssorts = false)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Given the blacklist provided, remove root items from assort
    /// </summary>
    /// <param name="assortToFilter">Trader assort to modify</param>
    /// <param name="itemsTplsToRemove">Item TPLs the assort should not have</param>
    protected void RemoveItemsFromAssort(TraderAssort assortToFilter, List<string> itemsTplsToRemove)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Reset every traders root item `BuyRestrictionCurrent` property to 0
    /// </summary>
    /// <param name="assortItems">Items to adjust</param>
    protected void ResetBuyRestrictionCurrentValue(List<Item> assortItems)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a dict of all assort id = quest id mappings used to work out what items should be shown to player based on the quests they've started/completed/failed
    /// </summary>
    protected void HydrateMergedQuestAssorts()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Reset a traders assorts and move nextResupply value to future
    /// Flag trader as needing a flea offer reset to be picked up by flea update() function
    /// </summary>
    /// <param name="trader">trader details to alter</param>
    public void ResetExpiredTrader(Trader trader)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Does the supplied trader need its assorts refreshed
    /// </summary>
    /// <param name="traderID">Trader to check</param>
    /// <returns>true they need refreshing</returns>
    public bool TraderAssortsHaveExpired(string traderID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get an array of pristine trader items prior to any alteration by player (as they were on server start)
    /// </summary>
    /// <param name="traderId">trader id</param>
    /// <returns>array of Items</returns>
    protected List<Item> GetPristineTraderAssorts(string traderId)
    {
        throw new NotImplementedException();
    }
}
