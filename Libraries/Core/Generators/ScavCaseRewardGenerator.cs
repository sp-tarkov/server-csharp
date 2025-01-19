using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Hideout;
using Core.Models.Spt.Hideout;

namespace Core.Generators;

[Injectable]
public class ScavCaseRewardGenerator()
{

    /// <summary>
    /// Create an array of rewards that will be given to the player upon completing their scav case build
    /// </summary>
    /// <param name="recipeId">recipe of the scav case craft</param>
    /// <returns>Product array</returns>
    public List<List<Product>> Generate(string recipeId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get all db items that are not blacklisted in scavcase config or global blacklist
    /// Store in class field
    /// </summary>
    protected void CacheDbItems()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Pick a number of items to be rewards, the count is defined by the values in `itemFilters` param
    /// </summary>
    /// <param name="items">item pool to pick rewards from</param>
    /// <param name="itemFilters">how the rewards should be filtered down (by item count)</param>
    /// <returns></returns>
    protected List<TemplateItem> PickRandomRewards(
        List<TemplateItem> items,
        RewardCountAndPriceDetails itemFilters,
        string rarity)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose if money should be a reward based on the moneyRewardChancePercent config chance in scavCaseConfig
    /// </summary>
    /// <returns>true if reward should be money</returns>
    protected bool RewardShouldBeMoney()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose if ammo should be a reward based on the ammoRewardChancePercent config chance in scavCaseConfig
    /// </summary>
    /// <returns>true if reward should be ammo</returns>
    protected bool RewardShouldBeAmmo()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose from rouble/dollar/euro at random
    /// </summary>
    protected TemplateItem GetRandomMoney()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a random ammo from items.json that is not in the ammo blacklist AND inside the price range defined in scavcase.json config
    /// </summary>
    /// <param name="rarity">The rarity this ammo reward is for</param>
    /// <returns>random ammo item from items.json</returns>
    protected TemplateItem GetRandomAmmo(string rarity)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Take all the rewards picked create the Product object array ready to return to calling code
    /// Also add a stack count to ammo and money
    /// </summary>
    /// <param name="rewardItems">items to convert</param>
    /// <returns>Product array</returns>
    protected List<List<TemplateItem>> RandomiseContainerItemRewards(List<TemplateItem> rewardItems, string rarity)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// </summary>
    /// <param name="dbItems">all items from the items.json</param>
    /// <param name="itemFilters">controls how the dbItems will be filtered and returned (handbook price)</param>
    /// <returns>filtered dbItems array</returns>
    protected List<TemplateItem> GetFilteredItemsByPrice(
        List<TemplateItem> dbItems,
        RewardCountAndPriceDetails itemFilters)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gathers the reward min and max count params for each reward quality level from config and scavcase.json into a single object
    /// </summary>
    /// <param name="scavCaseDetails">production.json/scavRecipes object</param>
    /// <returns>ScavCaseRewardCountsAndPrices object</returns>
    protected ScavCaseRewardCountsAndPrices GetScavCaseRewardCountsAndPrices(ScavRecipe scavCaseDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Randomises the size of ammo and money stacks
    /// </summary>
    /// <param name="itemToCalculate">ammo or money item</param>
    /// <param name="rarity">rarity (common/rare/superrare)</param>
    /// <returns>value to set stack count to</returns>
    protected int GetRandomAmountRewardForScavCase(TemplateItem itemToCalculate, string rarity)
    {
        throw new NotImplementedException();
    }
}
