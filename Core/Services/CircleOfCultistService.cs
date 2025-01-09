using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Spt.Config;
using Core.Models.Spt.Hideout;
using Hideout = Core.Models.Eft.Common.Tables.Hideout;

namespace Core.Services;

public class CircleOfCultistService
{
    /// <summary>
    /// Start a sacrifice event
    /// Generate rewards
    /// Delete sacrificed items
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile doing sacrifice</param>
    /// <param name="request">Client request</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse StartSacrifice(
        string sessionId,
        PmcData pmcData,
        HideoutCircleOfCultistProductionStartRequestData request
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Attempt to add all rewards to cultist circle, if they don't fit remove one and try again until they fit
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="rewards">Rewards to send to player</param>
    /// <param name="containerGrid">Cultist grid to add rewards to</param>
    /// <param name="cultistCircleStashId">Stash id</param>
    /// <param name="output">Client output</param>
    protected void AddRewardsToCircleContainer(
        string sessionId,
        PmcData pmcData,
        List<List<Item>> rewards,
        List<List<int>> containerGrid,
        string cultistCircleStashId,
        ItemEventRouterResponse output
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a map of the possible direct rewards, keyed by the items needed to be sacrificed
    /// </summary>
    /// <param name="directRewards">Direct rewards array from hideout config</param>
    /// <returns>Dictionary</returns>
    protected Dictionary<string, DirectRewardSettings> GenerateSacrificedItemsCache(List<DirectRewardSettings> directRewards)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the reward amount multiple value based on players hideout management skill + configs rewardPriceMultiplerMinMax values
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="cultistCircleSettings">Circle config settings</param>
    /// <returns>Reward Amount Multiplier</returns>
    protected double GetRewardAmountMultiplier(PmcData pmcData, CultistCircleSettings cultistCircleSettings)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Register production inside player profile
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="recipeId">Recipe id</param>
    /// <param name="sacrificedItems">Items player sacrificed</param>
    /// <param name="craftingTime">How long the ritual should take</param>
    protected void RegisterCircleOfCultistProduction(
        string sessionId,
        PmcData pmcData,
        string recipeId,
        List<Item> sacrificedItems,
        double craftingTime
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the circle craft time as seconds, value is based on reward item value
    /// And get the bonus status to determine what tier of reward is given
    /// </summary>
    /// <param name="rewardAmountRoubles">Value of rewards in roubles</param>
    /// <param name="circleConfig">Circle config values</param>
    /// <param name="directRewardSettings">OPTIONAL - Values related to direct reward being given</param>
    /// <returns>craft time + type of reward + reward details</returns>
    protected CircleCraftDetails GetCircleCraftingInfo(
        double rewardAmountRoubles,
        CultistCircleSettings circleConfig,
        DirectRewardSettings directRewardSettings = null
    )
    {
        throw new NotImplementedException();
    }

    protected CraftTimeThreshold GetMatchingThreshold(
        List<CraftTimeThreshold> thresholds,
        double rewardAmountRoubles
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the items player sacrificed in circle
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <returns>Array of items from player inventory</returns>
    protected List<Item> GetSacrificedItems(PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Given a pool of items + rouble budget, pick items until the budget is reached
    /// </summary>
    /// <param name="rewardItemTplPool">Items that can be picked</param>
    /// <param name="rewardBudget">Rouble budget to reach</param>
    /// <param name="cultistCircleStashId">Id of stash item</param>
    /// <returns>Array of item arrays</returns>
    protected List<List<Item>> GetRewardsWithinBudget(
        List<string> rewardItemTplPool,
        double rewardBudget,
        string cultistCircleStashId,
        CultistCircleSettings circleConfig
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get direct rewards
    /// </summary>
    /// <param name="sessionId">sessionId</param>
    /// <param name="directReward">Items sacrificed</param>
    /// <param name="cultistCircleStashId">Id of stash item</param>
    /// <returns>The reward object</returns>
    protected List<List<Item>> GetDirectRewards(
        string sessionId,
        DirectRewardSettings directReward,
        string cultistCircleStashId
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check for direct rewards from what player sacrificed
    /// </summary>
    /// <param name="sessionId">sessionId</param>
    /// <param name="sacrificedItems">Items sacrificed</param>
    /// <returns>Direct reward items to send to player</returns>
    protected DirectRewardSettings CheckForDirectReward(
        string sessionId,
        List<Item> sacrificedItems,
        Dictionary<string, DirectRewardSettings> directRewardsCache
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create an md5 key of the sacrificed + reward items
    /// </summary>
    /// <param name="directReward">Direct reward to create key for</param>
    /// <returns>Key</returns>
    protected string GetDirectRewardHashKey(DirectRewardSettings directReward)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Explicit rewards have their own stack sizes as they don't use a reward rouble pool
    /// </summary>
    /// <param name="rewardTpl">Item being rewarded to get stack size of</param>
    /// <returns>stack size of item</returns>
    protected int GetDirectRewardBaseTypeStackSize(string rewardTpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add a record to the player's profile to signal they have accepted a non-repeatable direct reward
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="directReward">Reward sent to player</param>
    protected void FlagDirectRewardAsAcceptedInProfile(string sessionId, DirectRewardSettings directReward)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the size of a reward item's stack
    /// 1 for everything except ammo, ammo can be between min stack and max stack
    /// </summary>
    /// <param name="itemTpl">Item chosen</param>
    /// <param name="rewardPoolRemaining">Rouble amount of pool remaining to fill</param>
    /// <returns>Size of stack</returns>
    protected int GetRewardStackSize(string itemTpl, int rewardPoolRemaining)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a pool of tpl IDs of items the player needs to complete hideout crafts/upgrade areas
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Profile of player who will be getting the rewards</param>
    /// <param name="rewardType">Do we return bonus items (hideout/task items)</param>
    /// <param name="cultistCircleConfig">Circle config</param>
    /// <returns>Array of tpls</returns>
    protected string[] GetCultistCircleRewardPool(
        string sessionId,
        PmcData pmcData,
        CircleCraftDetails craftingInfo,
        CultistCircleSettings cultistCircleConfig)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check player's profile for quests with hand-in requirements and add those required items to the pool
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="itemRewardBlacklist">Items not to add to pool</param>
    /// <param name="rewardPool">Pool to add items to</param>
    protected void AddTaskItemRequirementsToRewardPool(
        PmcData pmcData,
        HashSet<string> itemRewardBlacklist,
        HashSet<string> rewardPool)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adds items the player needs to complete hideout crafts/upgrades to the reward pool
    /// </summary>
    /// <param name="hideoutDbData">Hideout area data</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="itemRewardBlacklist">Items not to add to pool</param>
    /// <param name="rewardPool">Pool to add items to</param>
    protected void AddHideoutUpgradeRequirementsToRewardPool(
        Hideout hideoutDbData,
        PmcData pmcData,
        HashSet<string> itemRewardBlacklist,
        HashSet<string> rewardPool)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get all active hideout areas
    /// </summary>
    /// <param name="areas">Hideout areas to iterate over</param>
    /// <returns>Active area array</returns>
    protected BotHideoutArea[] GetPlayerAccessibleHideoutAreas(BotHideoutArea[] areas)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get array of random reward items
    /// </summary>
    /// <param name="rewardPool">Reward pool to add to</param>
    /// <param name="itemRewardBlacklist">Item tpls to ignore</param>
    /// <param name="itemsShouldBeHighValue">Should these items meet the valuable threshold</param>
    /// <returns>Set of item tpls</returns>
    protected HashSet<string> GenerateRandomisedItemsAndAddToRewardPool(
        HashSet<string> rewardPool,
        HashSet<string> itemRewardBlacklist,
        bool itemsShouldBeHighValue)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Iterate over passed in hideout requirements and return the Item
    /// </summary>
    /// <param name="requirements">Requirements to iterate over</param>
    /// <returns>Array of item requirements</returns>
    protected (StageRequirement[] StageRequirement, Requirement[] Requirement) GetItemRequirements(RequirementBase[] requirements)
    {
        throw new NotImplementedException();
    }
}
